using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL
{
    public class BookingBLL
    {
        private readonly BookingRepository _repo = new BookingRepository();
        private readonly TravelCompanyDbContext _context = new TravelCompanyDbContext();

        public List<Booking> GetAllBookings() => _repo.GetAll();

        public List<Booking> SearchBookings(string kw) => _repo.Search(kw);

        public void AddBooking(Booking b)
        {
            // Clear tracking cache to ensure we get fresh data from DB 
            // and avoid stale vehicle status issues
            _context.ChangeTracker.Clear();

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Ensure auto-release is run before checking availability
                    AutoReleaseVehicles();

                    // 1. Get Schedule for price and slot calculation
                    var schedule = _context.TourSchedules.Include(s => s.TourTemplate).FirstOrDefault(s => s.ScheduleId == b.ScheduleId);
                    if (schedule == null) throw new Exception("Schedule not found.");
                    if (schedule.AvailableSlots < b.NumPersons) 
                        throw new Exception($"Not enough slots available. Remaining: {schedule.AvailableSlots}");

                    // 2. Auto-calculate TotalAmount
                    b.TotalAmount = (schedule.TourTemplate?.PricePerPerson ?? 0) * b.NumPersons;
                    
                    b.TourId = schedule.TourTemplateId;
                    if (b.TourId == 0) throw new Exception("Internal Error: TourTemplateId is 0. Please check tour configuration.");

                    // Use provided BookingDate if set (not min value), otherwise use Now
                    if (b.BookingDate == default)
                        b.BookingDate = DateTime.Now;

                    // Ensure AccountId is valid (fallback to first available if none logged in)
                    if (b.AccountId <= 0)
                    {
                        var firstAccount = _context.Accounts.FirstOrDefault();
                        b.AccountId = firstAccount?.AccountId ?? 1;
                    }

                    b.CreatedAt = DateTime.Now;
                    b.UpdatedAt = DateTime.Now;
                    b.Status = "Confirmed";
                    b.BookingCode = "BK" + DateTime.Now.ToString("yyyyMMddHHmmss");

                    // 3. Update Tour Slots
                    schedule.AvailableSlots -= b.NumPersons;
                    schedule.UpdatedAt = DateTime.Now;

                    // 4. Auto-assign Vehicles (Gap-based Strategy)
                    var currentTourVehicles = _context.TourVehicles
                        .Include(tv => tv.Vehicle)
                        .Where(tv => tv.ScheduleId == b.ScheduleId)
                        .ToList();

                    int currentlyAssignedCapacity = currentTourVehicles.Sum(tv => tv.Vehicle.Capacity);
                    
                    int totalBookedNumPersons = _context.Bookings
                        .Where(bk => bk.ScheduleId == b.ScheduleId && bk.Status != "Cancelled")
                        .Sum(bk => (int?)bk.NumPersons) ?? 0;
                    
                    totalBookedNumPersons += b.NumPersons;

                    if (currentlyAssignedCapacity >= totalBookedNumPersons)
                    {
                        _context.Bookings.Add(b);
                        _context.SaveChanges();
                        transaction.Commit();
                        return;
                    }

                    int neededMore = totalBookedNumPersons - currentlyAssignedCapacity;
                    var availableVehicles = _context.Vehicles.Where(v => v.Status == "Available").ToList();

                    if (!availableVehicles.Any())
                        throw new Exception($"Need {neededMore} more capacity, but no available vehicles found!");

                    int minCap = availableVehicles.Min(v => v.Capacity);
                    var toAssign = new List<Vehicle>();
                    int assignedCapacity = 0;

                    var bestSingle = availableVehicles
                        .Where(v => v.Capacity >= neededMore && (v.Capacity - neededMore) <= minCap)
                        .OrderBy(v => v.Capacity)
                        .FirstOrDefault();

                    if (bestSingle != null)
                    {
                        toAssign.Add(bestSingle);
                        assignedCapacity = bestSingle.Capacity;
                    }
                    else
                    {
                        var tempAvailable = availableVehicles.OrderByDescending(v => v.Capacity).ToList();
                        int remaining = neededMore;
                        while (remaining > 0 && tempAvailable.Any())
                        {
                            var v = tempAvailable.FirstOrDefault(x => x.Capacity <= remaining) ?? tempAvailable.First();
                            toAssign.Add(v);
                            assignedCapacity += v.Capacity;
                            remaining -= v.Capacity;
                            tempAvailable.Remove(v);
                        }
                    }

                    if (assignedCapacity < neededMore)
                        throw new Exception($"Not enough available capacity. Need: {neededMore}, Available: {assignedCapacity}");

                    foreach (var v in toAssign)
                    {
                        _context.TourVehicles.Add(new TourVehicle { 
                            TourId = b.TourId, // Use the SAME TourId as booking
                            ScheduleId = b.ScheduleId, 
                            VehicleId = v.VehicleId 
                        });
                        v.Status = "Busy";
                    }

                    _context.Bookings.Add(b);
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void AutoReleaseVehicles()
        {
            try
            {
                using (var context = new TravelCompanyDbContext())
                {
                    var today = DateTime.Today;
                    
                    
                    var bookings = context.Bookings
                        .Include(b => b.TourSchedule).ThenInclude(s => s.TourTemplate)
                        .Where(b => b.Status == "Confirmed")
                        .ToList();

                    var completedBookings = bookings
                        .Where(b => b.TourSchedule.ReturnDate.HasValue && b.TourSchedule.ReturnDate.Value.ToDateTime(TimeOnly.MinValue) < today)
                        .ToList();

                    if (completedBookings.Any())
                    {
                        foreach (var b in completedBookings)
                        {
                            b.Status = "Completed";
                            b.UpdatedAt = DateTime.Now;

                            // Update Schedule status too
                            b.TourSchedule.Status = "Completed";
                            b.TourSchedule.UpdatedAt = DateTime.Now;

                            var vIds = context.TourVehicles
                                .Where(tv => tv.ScheduleId == b.ScheduleId)
                                .Select(tv => tv.VehicleId)
                                .ToList();

                            var vehiclesToRelease = context.Vehicles
                                .Where(v => vIds.Contains(v.VehicleId))
                                .ToList();

                            foreach (var v in vehiclesToRelease)
                            {
                                v.Status = "Available";
                            }
                        }
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AutoReleaseVehicles: {ex.Message}");
            }
        }

        public void UpdateBookingStatus(int bookingId, string status, string? reason = null)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var b = _context.Bookings.Find(bookingId);
                    if (b == null) throw new Exception("Booking not found.");

                    if (status == "Cancelled" && b.Status != "Cancelled")
                    {
                       
                        var schedule = _context.TourSchedules.Find(b.ScheduleId);
                        if (schedule != null)
                        {
                            schedule.AvailableSlots += b.NumPersons;
                        }
                        b.CancelledAt = DateTime.Now;
                        b.CancelReason = reason;

                        ReleaseVehiclesForTour(b.ScheduleId, false);
                    }
                    else if (status == "Completed" && b.Status != "Completed")
                    {
                        ReleaseVehiclesForTour(b.ScheduleId, true);
                    }

                    b.Status = status;
                    b.UpdatedAt = DateTime.Now;

                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }


        private void ReleaseVehiclesForTour(int scheduleId, bool completeComponent = true)
        {
            if (completeComponent)
            {
                var schedule = _context.TourSchedules.Find(scheduleId);
                if (schedule != null)
                {
                    schedule.Status = "Completed";
                    schedule.UpdatedAt = DateTime.Now;
                }
            }

            var tourVehicles = _context.TourVehicles
                .Include(tv => tv.Vehicle)
                .Where(tv => tv.ScheduleId == scheduleId)
                .ToList();

            foreach (var tv in tourVehicles)
            {
                tv.Vehicle.Status = "Available";
            }
        }
        
        public List<Customer> GetCustomers()
        {
            return _context.Customers.OrderBy(c => c.FullName).ToList();
        }

        public List<TourSchedule> GetActiveSchedules()
        {
            return _context.TourSchedules.Include(s => s.TourTemplate).Where(s => s.Status == "Active" && !s.IsDeleted).OrderBy(s => s.TourTemplate.TourName).ToList();
        }

        public List<Vehicle> GetAvailableVehicles()
        {
            return _context.Vehicles.Where(v => v.Status == "Available").ToList();
        }

        public List<Vehicle> GetVehiclesBySchedule(int scheduleId)
        {
            return _context.TourVehicles
                .Include(tv => tv.Vehicle)
                .Where(tv => tv.ScheduleId == scheduleId)
                .Select(tv => tv.Vehicle)
                .ToList();
        }
    }
}
