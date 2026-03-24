using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL
{
    public class CustomerRepository
    {
        private readonly string _connectionString;

        public CustomerRepository()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnectionString")
                ?? "Server=localhost\\SQLEXPRESS;database=TravelCompanyDB;uid=sa;pwd=123456;TrustServerCertificate=True;";
        }

        public List<Customer> GetAll()
        {
            var list = new List<Customer>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = @"SELECT * FROM Customers WHERE IsDeleted = 0 ORDER BY CreatedAt DESC";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapToCustomer(reader));
            return list;
        }

        public List<Customer> Search(string keyword)
        {
            var list = new List<Customer>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = @"SELECT * FROM Customers 
                        WHERE IsDeleted = 0 
                          AND (FullName LIKE @kw OR Phone LIKE @kw)
                        ORDER BY CreatedAt DESC";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@kw", $"%{keyword}%");
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapToCustomer(reader));
            return list;
        }

        public Customer? GetById(int customerId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "SELECT * FROM Customers WHERE CustomerId = @Id AND IsDeleted = 0";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", customerId);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapToCustomer(reader) : null;
        }

        public void Add(Customer c)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = @"INSERT INTO Customers 
                            (FullName, Phone, Email, DateOfBirth, Address, PassportNo, Notes, CreatedAt, UpdatedAt, IsDeleted)
                        VALUES 
                            (@FullName, @Phone, @Email, @Dob, @Address, @Passport, @Notes, @Created, @Updated, 0)";
            using var cmd = new SqlCommand(sql, conn);
            BindParams(cmd, c);
            cmd.Parameters.AddWithValue("@Created", DateTime.Now);
            cmd.Parameters.AddWithValue("@Updated", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        public void Update(Customer c)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = @"UPDATE Customers SET
                            FullName   = @FullName,
                            Phone      = @Phone,
                            Email      = @Email,
                            DateOfBirth= @Dob,
                            Address    = @Address,
                            PassportNo = @Passport,
                            Notes      = @Notes,
                            UpdatedAt  = @Updated
                        WHERE CustomerId = @Id AND IsDeleted = 0";
            using var cmd = new SqlCommand(sql, conn);
            BindParams(cmd, c);
            cmd.Parameters.AddWithValue("@Updated", DateTime.Now);
            cmd.Parameters.AddWithValue("@Id", c.CustomerId);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int customerId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "UPDATE Customers SET IsDeleted = 1, UpdatedAt = @Updated WHERE CustomerId = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", customerId);
            cmd.Parameters.AddWithValue("@Updated", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        public bool IsPhoneDuplicate(string phone, int excludeId = 0)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = @"SELECT COUNT(1) FROM Customers 
                        WHERE Phone = @Phone AND IsDeleted = 0 AND CustomerId != @ExcludeId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Phone", phone);
            cmd.Parameters.AddWithValue("@ExcludeId", excludeId);
            return (int)cmd.ExecuteScalar() > 0;
        }

        public bool HasBookings(int customerId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "SELECT COUNT(1) FROM Bookings WHERE CustomerId = @Id AND Status != 'Cancelled'";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", customerId);
            return (int)cmd.ExecuteScalar() > 0;
        }

        // ── helpers ──────────────────────────────────────────────────────────
        private static void BindParams(SqlCommand cmd, Customer c)
        {
            cmd.Parameters.AddWithValue("@FullName", c.FullName ?? string.Empty);
            cmd.Parameters.AddWithValue("@Phone", c.Phone    ?? string.Empty);
            cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(c.Email) ? DBNull.Value : c.Email);
            cmd.Parameters.AddWithValue("@Dob", c.DateOfBirth.HasValue ? c.DateOfBirth.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(c.Address) ? DBNull.Value : c.Address);
            cmd.Parameters.AddWithValue("@Passport", string.IsNullOrEmpty(c.PassportNo) ? DBNull.Value : c.PassportNo);
            cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(c.Notes) ? DBNull.Value : c.Notes);
        }

        private static Customer MapToCustomer(SqlDataReader r) => new Customer
        {
            CustomerId  = (int)r["CustomerId"],
            FullName    = r["FullName"].ToString() ?? "",
            Phone       = r["Phone"].ToString()    ?? "",
            Email       = r["Email"]      == DBNull.Value ? null : r["Email"].ToString(),
            DateOfBirth = r["DateOfBirth"]== DBNull.Value ? null : (DateTime?)r["DateOfBirth"],
            Address     = r["Address"]    == DBNull.Value ? null : r["Address"].ToString(),
            PassportNo  = r["PassportNo"] == DBNull.Value ? null : r["PassportNo"].ToString(),
            Notes       = r["Notes"]      == DBNull.Value ? null : r["Notes"].ToString(),
            IsDeleted   = (bool)r["IsDeleted"],
        };
    }
}