using System;
using System.Collections.Generic;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL
{
    public class PaymentBLL
    {
        private readonly PaymentRepository _repo = new PaymentRepository();

        public List<Payment> GetAllPayments() => _repo.GetAll();

        public List<Booking> GetUnpaidBookings() => _repo.GetUnpaidBookings();

        public void CreatePayment(Payment p)
        {
            // Check if payment already exists for this booking
            var existing = _repo.GetUnpaidBookings();
            if (!existing.Any(b => b.BookingId == p.BookingId))
            {
                // This means the booking is already tagged as having a payment in our filtered GetUnpaidBookings
                // or it's simply not eligible.
                throw new Exception("This booking already has a payment registered or is ineligible.");
            }

            p.CreatedAt = DateTime.Now;
            p.UpdatedAt = DateTime.Now;
            p.PaymentDate = DateTime.Now;
            p.Status = "Pending";
            
            _repo.Add(p);
        }

        public bool ProcessPayment(int paymentId)
        {
            var p = _repo.GetById(paymentId);
            if (p == null) return false;

            // Business Logic: Simulation of payment processing
            p.Status = "Paid";
            p.PaidAt = DateTime.Now;
            p.UpdatedAt = DateTime.Now;
            p.TransactionRef = "SIM-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

            _repo.Update(p);
            return true;
        }

        public List<Payment> SearchPayments(string kw) => _repo.Search(kw);
    }
}
