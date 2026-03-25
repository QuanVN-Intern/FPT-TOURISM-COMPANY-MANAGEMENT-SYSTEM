using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL
{
    public class TourScheduleRepository
    {
        private readonly TravelCompanyDbContext _context = new TravelCompanyDbContext();

        public void Add(TourSchedule schedule)
        {
            _context.TourSchedules.Add(schedule);
            _context.SaveChanges();
        }

        public void Update(TourSchedule schedule)
        {
            _context.TourSchedules.Update(schedule);
            _context.SaveChanges();
        }

        public void Delete(int scheduleId)
        {
            var schedule = _context.TourSchedules.Find(scheduleId);
            if (schedule != null)
            {
                schedule.IsDeleted = true;
                schedule.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public TourSchedule? GetById(int scheduleId)
        {
            return _context.TourSchedules
                .Include(s => s.TourTemplate)
                .FirstOrDefault(s => s.ScheduleId == scheduleId && !s.IsDeleted);
        }

        public List<TourSchedule> GetSchedulesByTemplate(int templateId)
        {
            return _context.TourSchedules
                .Include(s => s.TourTemplate)
                .Where(s => s.TourTemplateId == templateId && !s.IsDeleted)
                .OrderBy(s => s.DepartureDate)
                .ToList();
        }

        public List<TourSchedule> GetAllSchedules()
        {
            return _context.TourSchedules
                .Include(s => s.TourTemplate)
                .Where(s => !s.IsDeleted && !s.TourTemplate.IsDeleted)
                .OrderBy(s => s.DepartureDate)
                .ToList();
        }

        public bool CheckOverlap(int templateId, DateOnly departureDate, DateOnly returnDate, int? excludeId = null)
        {
            var query = _context.TourSchedules
                .Where(s => s.TourTemplateId == templateId && !s.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(s => s.ScheduleId != excludeId.Value);

            // Overlap condition: S1 <= E2 AND S2 <= E1
            return query.Any(s => departureDate <= s.ReturnDate && s.DepartureDate <= returnDate);
        }

        public bool HasBookings(int scheduleId)
        {
            return _context.Bookings.Any(b => b.ScheduleId == scheduleId && b.Status != "Cancelled");
        }

        public int GetTotalBookedSlots(int scheduleId)
        {
            return _context.Bookings
                .Where(b => b.ScheduleId == scheduleId && b.Status != "Cancelled")
                .Sum(b => (int?)b.NumPersons) ?? 0;
        }

        public void UpdateAvailableSlots(int scheduleId, int numPeople)
        {
            // Use execute update for atomic operation
            _context.Database.ExecuteSqlRaw(
                "UPDATE TourSchedules SET AvailableSlots = AvailableSlots - {0} WHERE ScheduleId = {1} AND AvailableSlots >= {0}",
                numPeople, scheduleId);
        }
    }
}
