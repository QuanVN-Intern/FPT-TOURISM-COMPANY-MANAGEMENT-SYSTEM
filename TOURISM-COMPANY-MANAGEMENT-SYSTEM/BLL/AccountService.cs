using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL
{
    public class AccountService
    {
        private readonly AccountRepository _repo = new AccountRepository();

        // ── Auth ─────────────────────────────────────────────────────────────

        public bool Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            var hash    = HashPassword(password);
            var account = _repo.Login(username, hash);
            if (account == null) return false;

            AuthSession.SetUser(account);
            return true;
        }

        public void Logout() => AuthSession.Clear();

        // ── CRUD ─────────────────────────────────────────────────────────────

        public List<Account> GetAll()   => _repo.GetAll();
        public List<Role>    GetRoles() => _repo.GetAllRoles();

        /// <summary>
        /// Returns roles that can be assigned.
        /// Admin is excluded — only one Admin allowed in the system.
        /// </summary>
        public List<Role> GetAssignableRoles()
        {
            var all = _repo.GetAllRoles();
            all.RemoveAll(r => r.RoleName == "Admin");
            return all;
        }

        public void CreateAccount(Account a, string plainPassword)
        {
            // New accounts can never be Admin
            var roles = _repo.GetAllRoles();
            var selectedRole = roles.Find(r => r.RoleId == a.RoleId);
            if (selectedRole?.RoleName == "Admin")
                throw new Exception("Cannot create a new Admin account. There is only one Admin in the system.");

            ValidateAccount(a, isEdit: false);

            if (string.IsNullOrWhiteSpace(plainPassword) || plainPassword.Length < 6)
                throw new Exception("Password must be at least 6 characters.");

            a.PasswordHash = HashPassword(plainPassword);
            a.IsActive     = true;
            _repo.Add(a);
        }

        public void UpdateAccount(Account a)
        {
            var existing = _repo.GetById(a.AccountId)
                ?? throw new Exception("Account not found.");

            var roles = _repo.GetAllRoles();
            var selectedRole = roles.Find(r => r.RoleId == a.RoleId);

            // Cannot promote anyone to Admin
            if (selectedRole?.RoleName == "Admin" && existing.RoleName != "Admin")
                throw new Exception("Cannot assign the Admin role to another account. There is only one Admin.");

            // Cannot demote the last Admin
            if (existing.RoleName == "Admin" && a.RoleId != existing.RoleId)
            {
                int adminCount = _repo.GetAll().FindAll(x => x.RoleName == "Admin" && x.IsActive).Count;
                if (adminCount <= 1)
                    throw new Exception("Cannot change the role of the last active Admin account.");
            }

            ValidateAccount(a, isEdit: true);
            _repo.Update(a);
        }

        public void ResetPassword(int accountId, string newPlainPassword)
        {
            if (string.IsNullOrWhiteSpace(newPlainPassword) || newPlainPassword.Length < 6)
                throw new Exception("New password must be at least 6 characters.");
            _repo.UpdatePassword(accountId, HashPassword(newPlainPassword));
        }

        public void ToggleActive(int accountId, bool active)
        {
            var account = _repo.GetById(accountId)
                ?? throw new Exception("Account not found.");

            if (!active && account.RoleName == "Admin")
            {
                int adminCount = _repo.GetAll().FindAll(x => x.RoleName == "Admin" && x.IsActive).Count;
                if (adminCount <= 1)
                    throw new Exception("Cannot deactivate the last active Admin account.");
            }
            _repo.SetActive(accountId, active);
        }

        public void DeleteAccount(int accountId)
        {
            var account = _repo.GetById(accountId)
                ?? throw new Exception("Account not found.");

            if (account.RoleName == "Admin")
                throw new Exception("Cannot delete the Admin account.");

            if (account.AccountId == AuthSession.Current?.AccountId)
                throw new Exception("Cannot delete your own account.");

            _repo.Delete(accountId);
        }

        // ── Tour Guide helpers ─────────────────────────────────────────────────

        public void AddTourGuide(Account a)
        {
            if (string.IsNullOrWhiteSpace(a.FullName))
                throw new Exception("Full Name is required.");
            if (string.IsNullOrWhiteSpace(a.Username))
                throw new Exception("Username is required.");
            if (_repo.IsUsernameDuplicate(a.Username))
                throw new Exception($"Username '{a.Username}' is already taken.");
            if (!string.IsNullOrEmpty(a.Email) && _repo.IsEmailDuplicate(a.Email))
                throw new Exception("Email is already in use by another account.");
            if (string.IsNullOrEmpty(a.PasswordHash))
                throw new Exception("Password is required.");

            int guideRoleId = _repo.GetAllRoles().Find(r => r.RoleName == "Tour Guide")?.RoleId
                ?? throw new Exception("Role 'Tour Guide' not found in database.");
            a.RoleId = guideRoleId;
            a.IsActive = true;
            _repo.Add(a);
        }

        public void UpdateTourGuide(Account a)
        {
            var existing = _repo.GetById(a.AccountId)
                ?? throw new Exception("Account not found.");
            if (string.IsNullOrWhiteSpace(a.FullName))
                throw new Exception("Full Name is required.");
            if (_repo.IsUsernameDuplicate(a.Username, a.AccountId))
                throw new Exception($"Username '{a.Username}' is already taken.");
            if (!string.IsNullOrEmpty(a.Email) && _repo.IsEmailDuplicate(a.Email, a.AccountId))
                throw new Exception("Email is already in use by another account.");

            a.RoleId = existing.RoleId; // keep role
            if (!string.IsNullOrEmpty(a.PasswordHash))
                _repo.UpdatePassword(a.AccountId, a.PasswordHash);
            _repo.Update(a);
        }

        public void SoftDelete(int accountId) => _repo.Delete(accountId);

        // ── Validation ────────────────────────────────────────────────────────

        private void ValidateAccount(Account a, bool isEdit)
        {
            var errors = new List<string>();

            if (!isEdit)
            {
                if (string.IsNullOrWhiteSpace(a.Username))
                    errors.Add("Username is required.");
                else if (a.Username.Length < 3 || a.Username.Length > 50)
                    errors.Add("Username must be 3–50 characters.");
                else if (_repo.IsUsernameDuplicate(a.Username, 0))
                    errors.Add($"Username '{a.Username}' is already taken.");
            }

            if (string.IsNullOrWhiteSpace(a.FullName))
                errors.Add("Full name is required.");

            if (string.IsNullOrWhiteSpace(a.Email))
                errors.Add("Email is required.");
            else if (!Regex.IsMatch(a.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                errors.Add("Email format is invalid.");
            else if (_repo.IsEmailDuplicate(a.Email, isEdit ? a.AccountId : 0))
                errors.Add("Email is already in use by another account.");

            if (a.RoleId <= 0)
                errors.Add("A role must be selected.");

            // Age validation: employee must be at least 18 years old
            if (a.DateOfBirth.HasValue)
            {
                var today = DateTime.Today;
                int age   = today.Year - a.DateOfBirth.Value.Year;
                if (a.DateOfBirth.Value.Date > today.AddYears(-age)) age--;
                if (a.DateOfBirth.Value > today)
                    errors.Add("Date of birth cannot be in the future.");
                else if (age < 18)
                    errors.Add("Employee must be at least 18 years old.");
            }

            if (errors.Count > 0)
                throw new Exception("Validation Error(s):\n- " + string.Join("\n- ", errors));
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        public static string HashPassword(string plain)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}