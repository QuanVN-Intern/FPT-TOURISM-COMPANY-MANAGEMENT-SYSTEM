using System;
using System.Collections.Generic;
using System.Linq;
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
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 1. Get Tour for price and slot calculation
                    var tour = _context.Tours.Find(b.TourId);
                    if (tour == null) throw new Exception("Tour not found.");
                    if (tour.AvailableSlots < b.NumPersons) 
                        throw new Exception($"Not enough slots available. Remaining: {tour.AvailableSlots}");

                    // 2. Auto-calculate TotalAmount
                    b.TotalAmount = tour.PricePerPerson * b.NumPersons;
                    b.BookingDate = DateTime.Now;
                    b.CreatedAt = DateTime.Now;
                    b.UpdatedAt = DateTime.Now;
                    b.Status = "Confirmed";
                    b.BookingCode = "BK" + DateTime.Now.ToString("yyyyMMddHHmmss");

                    // 3. Update Tour Slots
                    tour.AvailableSlots -= b.NumPersons;
                    tour.UpdatedAt = DateTime.Now;

                    // 4. Auto-assign Vehicle
                    var vehicle = _context.Vehicles
                        .Where(v => v.Status == "Available" && v.Capacity >= b.NumPersons)
                        .OrderBy(v => v.Capacity)
                        .FirstOrDefault();

                    if (vehicle != null)
                    {
                        // Assign vehicle to tour if not already assigned within this context?
                        // Actually, the requirement says "auto assign xe". 
                        // Typically this means creating a TourVehicle entry.
                        var tourVehicle = new TourVehicle
                        {
                            TourId = b.TourId,
                            VehicleId = vehicle.VehicleId
                        };
                        _context.TourVehicles.Add(tourVehicle);
                        
                        // Update vehicle status? Depends on business rule. 
                        // Usually vehicle is "Busy" for that tour duration.
                        vehicle.Status = "Busy";
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
                        // Restore Tour Slots
                        var tour = _context.Tours.Find(b.TourId);
                        if (tour != null)
                        {
                            tour.AvailableSlots += b.NumPersons;
                        }
                        b.CancelledAt = DateTime.Now;
                        b.CancelReason = reason;
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
        
        public List<Customer> GetCustomers()
        {
            return _context.Customers.OrderBy(c => c.FullName).ToList();
        }

        public List<Tour> GetActiveTours()
        {
            return _context.Tours.Where(t => t.Status == "Active" && !t.IsDeleted).OrderBy(t => t.TourName).ToList();
        }
    }
}
