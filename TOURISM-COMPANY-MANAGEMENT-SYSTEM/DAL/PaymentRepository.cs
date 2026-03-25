using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL
{
    public class PaymentRepository
    {
        public List<Payment> GetAll()
        {
            using (var context = new TravelCompanyDbContext())
            {
                return context.Payments
                    .Include(p => p.Booking)
                    .ThenInclude(b => b.Customer)
                    .Include(p => p.Booking)
                    .ThenInclude(b => b.Tour)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();
            }
        }

        public Payment? GetById(int id)
        {
            using (var context = new TravelCompanyDbContext())
            {
                return context.Payments
                    .Include(p => p.Booking)
                    .FirstOrDefault(p => p.PaymentId == id);
            }
        }

        public void Add(Payment payment)
        {
            using (var context = new TravelCompanyDbContext())
            {
                context.Payments.Add(payment);
                context.SaveChanges();
            }
        }

        public void Update(Payment payment)
        {
            using (var context = new TravelCompanyDbContext())
            {
                context.Payments.Update(payment);
                context.SaveChanges();
            }
        }

        public List<Payment> Search(string kw)
        {
            using (var context = new TravelCompanyDbContext())
            {
                kw = kw.ToLower();
                return context.Payments
                    .Include(p => p.Booking)
                    .ThenInclude(b => b.Customer)
                    .Where(p => p.Booking.BookingCode.ToLower().Contains(kw) || 
                                p.Booking.Customer.FullName.ToLower().Contains(kw) ||
                                (p.TransactionRef != null && p.TransactionRef.ToLower().Contains(kw)))
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();
            }
        }

        public List<Booking> GetUnpaidBookings()
        {
            using (var context = new TravelCompanyDbContext())
            {
                return context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.Tour)
                    .Where(b => b.Status == "Confirmed" && !b.IsDeleted)
                    .ToList();
            }
        }
    }
}
