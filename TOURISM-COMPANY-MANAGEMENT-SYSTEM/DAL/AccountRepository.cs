using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL
{
    public class AccountRepository
    {
        private readonly string _connectionString;

        public AccountRepository()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnectionString")
                ?? "Server=localhost\\SQLEXPRESS;database=TravelCompanyDB;uid=sa;pwd=123456;TrustServerCertificate=True;";
        }

        public Account? Login(string username, string passwordHash)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = @"SELECT a.*, r.RoleName
                        FROM   Accounts a
                        JOIN   Roles    r ON a.RoleId = r.RoleId
                        WHERE  a.Username     = @Username
                          AND  a.PasswordHash = @PasswordHash
                          AND  a.IsActive     = 1
                          AND  a.IsDeleted    = 0";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapToAccount(reader) : null;
        }

        public List<Account> GetAll()
        {
            var list = new List<Account>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = @"SELECT a.*, r.RoleName
                        FROM   Accounts a
                        JOIN   Roles    r ON a.RoleId = r.RoleId
                        WHERE  a.IsDeleted = 0
                        ORDER BY a.CreatedAt DESC";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapToAccount(reader));
            return list;
        }

        public Account? GetById(int accountId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = @"SELECT a.*, r.RoleName
                        FROM   Accounts a
                        JOIN   Roles    r ON a.RoleId = r.RoleId
                        WHERE  a.AccountId = @Id AND a.IsDeleted = 0";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", accountId);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapToAccount(reader) : null;
        }

        public void Add(Account a)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            // LicenseNumber removed — not a DB column
            var sql = @"INSERT INTO Accounts
                            (Username, PasswordHash, FullName, Email, RoleId, IsActive, CreatedAt, UpdatedAt, IsDeleted)
                        VALUES
                            (@Username, @PasswordHash, @FullName, @Email, @RoleId, @IsActive, @Created, @Updated, 0)";
            using var cmd = new SqlCommand(sql, conn);
            BindParams(cmd, a);
            cmd.Parameters.AddWithValue("@Created", DateTime.Now);
            cmd.Parameters.AddWithValue("@Updated", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        public void Update(Account a)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            // LicenseNumber removed — not a DB column
            var sql = @"UPDATE Accounts SET
                            FullName  = @FullName,
                            Email     = @Email,
                            RoleId    = @RoleId,
                            IsActive  = @IsActive,
                            UpdatedAt = @UpdatedAt
                        WHERE AccountId = @Id AND IsDeleted = 0";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@FullName", a.FullName ?? string.Empty);
            cmd.Parameters.AddWithValue("@Email", a.Email    ?? string.Empty);
            cmd.Parameters.AddWithValue("@RoleId", a.RoleId);
            cmd.Parameters.AddWithValue("@IsActive", a.IsActive);
            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
            cmd.Parameters.AddWithValue("@Id", a.AccountId);
            cmd.ExecuteNonQuery();
        }

        public void UpdatePassword(int accountId, string newHash)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "UPDATE Accounts SET PasswordHash = @Hash, UpdatedAt = @Updated WHERE AccountId = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Hash", newHash);
            cmd.Parameters.AddWithValue("@Updated", DateTime.Now);
            cmd.Parameters.AddWithValue("@Id", accountId);
            cmd.ExecuteNonQuery();
        }

        public void SetActive(int accountId, bool active)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "UPDATE Accounts SET IsActive = @Active, UpdatedAt = @Updated WHERE AccountId = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Active", active);
            cmd.Parameters.AddWithValue("@Updated", DateTime.Now);
            cmd.Parameters.AddWithValue("@Id", accountId);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int accountId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "UPDATE Accounts SET IsDeleted = 1, UpdatedAt = @Updated WHERE AccountId = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", accountId);
            cmd.Parameters.AddWithValue("@Updated", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        public List<Account> GetAllDrivers()
        {
            var list = new List<Account>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = @"SELECT a.*, r.RoleName
                        FROM   Accounts a
                        JOIN   Roles    r ON a.RoleId = r.RoleId
                        WHERE  a.IsDeleted = 0 AND UPPER(TRIM(r.RoleName)) = 'DRIVER'
                        ORDER BY a.FullName";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapToAccount(reader));
            return list;
        }

        public List<Account> GetTourGuides()
        {
            var list = new List<Account>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = @"SELECT a.*, r.RoleName
                        FROM   Accounts a
                        JOIN   Roles    r ON a.RoleId = r.RoleId
                        WHERE  a.IsDeleted = 0 AND r.RoleName = 'Tour Guide'
                        ORDER BY a.FullName";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapToAccount(reader));
            return list;
        }

        public bool IsUsernameDuplicate(string username, int excludeId = 0)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "SELECT COUNT(1) FROM Accounts WHERE Username = @Username AND IsDeleted = 0 AND AccountId != @ExcludeId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@ExcludeId", excludeId);
            return (int)cmd.ExecuteScalar() > 0;
        }

        public bool IsEmailDuplicate(string email, int excludeId = 0)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "SELECT COUNT(1) FROM Accounts WHERE Email = @Email AND IsDeleted = 0 AND AccountId != @ExcludeId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@ExcludeId", excludeId);
            return (int)cmd.ExecuteScalar() > 0;
        }

        public List<Role> GetAllRoles()
        {
            var list = new List<Role>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "SELECT RoleId, RoleName FROM Roles WHERE IsDeleted = 0 ORDER BY RoleId";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new Role
                {
                    RoleId   = (int)reader["RoleId"],
                    RoleName = reader["RoleName"].ToString() ?? ""
                });
            return list;
        }

        // ── helpers ───────────────────────────────────────────────────────────

        private static void BindParams(SqlCommand cmd, Account a)
        {
            cmd.Parameters.AddWithValue("@Username", a.Username     ?? string.Empty);
            cmd.Parameters.AddWithValue("@PasswordHash", a.PasswordHash ?? string.Empty);
            cmd.Parameters.AddWithValue("@FullName", a.FullName     ?? string.Empty);
            cmd.Parameters.AddWithValue("@Email", a.Email        ?? string.Empty);
            cmd.Parameters.AddWithValue("@RoleId", a.RoleId);
            cmd.Parameters.AddWithValue("@IsActive", a.IsActive);
            // LicenseNumber intentionally excluded — not a DB column
        }

        private static Account MapToAccount(SqlDataReader r) => new Account
        {
            AccountId    = (int)r["AccountId"],
            Username     = r["Username"].ToString()     ?? "",
            PasswordHash = r["PasswordHash"].ToString() ?? "",
            FullName     = r["FullName"].ToString()     ?? "",
            Email        = r["Email"].ToString()        ?? "",
            RoleId       = (int)r["RoleId"],
            RoleName     = r["RoleName"].ToString()     ?? "",
            IsActive     = (bool)r["IsActive"],
            IsDeleted    = (bool)r["IsDeleted"],
            LicenseNumber = null   // not a DB column — always null from DB
        };
    }
}