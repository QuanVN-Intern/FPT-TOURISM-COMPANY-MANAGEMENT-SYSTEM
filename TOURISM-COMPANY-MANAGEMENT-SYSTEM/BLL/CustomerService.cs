using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL
{
    public class CustomerService
    {
        private readonly CustomerRepository _repo = new CustomerRepository();

        public List<Customer> GetAll() => _repo.GetAll();
        public List<Customer> Search(string kw) => _repo.Search(kw);
        public Customer? GetById(int id) => _repo.GetById(id);

        public void AddCustomer(Customer c)
        {
            Validate(c, isEdit: false);
            _repo.Add(c);
        }

        public void UpdateCustomer(Customer c)
        {
            if (_repo.GetById(c.CustomerId) == null)
                throw new Exception("Customer not found.");
            Validate(c, isEdit: true);
            _repo.Update(c);
        }

        public void DeleteCustomer(int customerId)
        {
            var c = _repo.GetById(customerId)
                ?? throw new Exception("Customer not found.");

            if (_repo.HasBookings(customerId))
                throw new Exception("Cannot delete a customer who has active bookings.");

            _repo.Delete(customerId);
        }

        // ── Validation ────────────────────────────────────────────────────────

        private void Validate(Customer c, bool isEdit)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(c.FullName))
                errors.Add("Full name is required.");
            else if (c.FullName.Length > 150)
                errors.Add("Full name cannot exceed 150 characters.");

            if (string.IsNullOrWhiteSpace(c.Phone))
                errors.Add("Phone number is required.");
            else if (!Regex.IsMatch(c.Phone, @"^\+?[0-9]{9,15}$"))
                errors.Add("Phone must be 9–15 digits (optional leading +).");
            else if (_repo.IsPhoneDuplicate(c.Phone, isEdit ? c.CustomerId : 0))
                errors.Add($"Phone number '{c.Phone}' is already registered to another customer.");

            if (!string.IsNullOrWhiteSpace(c.Email) &&
                !Regex.IsMatch(c.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                errors.Add("Email format is invalid.");

            if (c.DateOfBirth.HasValue && c.DateOfBirth.Value > DateTime.Today)
                errors.Add("Date of birth cannot be in the future.");

            if (errors.Count > 0)
                throw new Exception("Validation Error(s):\n- " + string.Join("\n- ", errors));
        }
    }
}