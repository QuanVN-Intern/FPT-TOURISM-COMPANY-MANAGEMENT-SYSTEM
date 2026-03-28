using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL
{
    /// <summary>Handles CRUD operations for Tour Guide accounts.</summary>
    public class TourGuideService
    {
        private readonly AccountRepository _repo = new AccountRepository();

        public List<Account> GetAll() => _repo.GetTourGuides();

        public List<Account> GetActive() => _repo.GetTourGuides().FindAll(g => g.IsActive);

        public void Add(Account a, string plainPassword)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(a.FullName))
                errors.Add("Full Name is required.");
            if (string.IsNullOrWhiteSpace(a.Username))
                errors.Add("Username is required.");
            else if (_repo.IsUsernameDuplicate(a.Username))
                errors.Add($"Username '{a.Username}' is already taken.");
            if (!string.IsNullOrWhiteSpace(a.Email) && _repo.IsEmailDuplicate(a.Email))
                errors.Add("Email is already in use by another account.");
            if (string.IsNullOrWhiteSpace(plainPassword) || plainPassword.Length < 6)
                errors.Add("Password must be at least 6 characters.");

            if (errors.Count > 0)
                throw new Exception("Validation error(s):\n- " + string.Join("\n- ", errors));

            int guideRoleId = _repo.GetAllRoles().Find(r => r.RoleName == "Tour Guide")?.RoleId
                ?? throw new Exception("Role 'Tour Guide' not found in database. Please check the Roles table.");

            a.RoleId      = guideRoleId;
            a.IsActive    = true;
            a.PasswordHash = Hash(plainPassword);
            _repo.Add(a);
        }

        public void Update(Account a, string? newPlainPassword = null)
        {
            var existing = _repo.GetById(a.AccountId)
                ?? throw new Exception("Account not found.");

            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(a.FullName))
                errors.Add("Full Name is required.");
            if (_repo.IsUsernameDuplicate(a.Username, a.AccountId))
                errors.Add($"Username '{a.Username}' is already taken.");
            if (!string.IsNullOrWhiteSpace(a.Email) && _repo.IsEmailDuplicate(a.Email, a.AccountId))
                errors.Add("Email is already in use by another account.");
            if (!string.IsNullOrWhiteSpace(newPlainPassword) && newPlainPassword!.Length < 6)
                errors.Add("Password must be at least 6 characters.");

            if (errors.Count > 0)
                throw new Exception("Validation error(s):\n- " + string.Join("\n- ", errors));

            a.RoleId = existing.RoleId; // preserve role
            _repo.Update(a);
            if (!string.IsNullOrWhiteSpace(newPlainPassword))
                _repo.UpdatePassword(a.AccountId, Hash(newPlainPassword!));
        }

        public void Delete(int accountId) => _repo.Delete(accountId);

        public void SetActive(int accountId, bool active) => _repo.SetActive(accountId, active);

        private static string Hash(string input)
        {
            using var sha = SHA256.Create();
            var b = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(b).ToLower();
        }
    }
}
