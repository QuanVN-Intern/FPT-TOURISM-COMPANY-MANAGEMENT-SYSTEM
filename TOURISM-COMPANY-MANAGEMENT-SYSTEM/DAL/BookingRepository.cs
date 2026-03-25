using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL
{
    public class BookingRepository
    {
        public List<Booking> GetAll()
        {
            using (var context = new TravelCompanyDbContext())
            {
                return context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.TourSchedule)
                        .ThenInclude(ts => ts.TourTemplate)
                    .Where(b => !b.IsDeleted)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToList();
            }
        }

        public Booking? GetById(int id)
        {
            using (var context = new TravelCompanyDbContext())
            {
                return context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.TourSchedule)
                        .ThenInclude(ts => ts.TourTemplate)
                    .FirstOrDefault(b => b.BookingId == id && !b.IsDeleted);
            }
        }

        public List<Booking> Search(string kw)
        {
            using (var context = new TravelCompanyDbContext())
            {
                return context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.TourSchedule)
                        .ThenInclude(ts => ts.TourTemplate)
                    .Where(b => !b.IsDeleted && 
                               (b.BookingCode.Contains(kw) || b.Customer.FullName.Contains(kw)))
                    .OrderByDescending(b => b.CreatedAt)
                    .ToList();
            }
        }

        public void Add(Booking booking)
        {
            using (var context = new TravelCompanyDbContext())
            {
                context.Bookings.Add(booking);
                context.SaveChanges();
            }
        }

        public void Update(Booking booking)
        {
            using (var context = new TravelCompanyDbContext())
            {
                context.Bookings.Update(booking);
                context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            using (var context = new TravelCompanyDbContext())
            {
                var booking = context.Bookings.Find(id);
                if (booking != null)
                {
                    booking.IsDeleted = true;
                    booking.UpdatedAt = DateTime.Now;
                    context.SaveChanges();
                }
            }
        }
    }
}
