using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL
{
    public class DestinationRepository
    {
        private readonly string _connectionString;

        public DestinationRepository()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnectionString") 
                ?? "Server=DESKTOP-E9VL67H;database=TravelCompanyDB;uid=sa;pwd=123456;TrustServerCertificate=True;";
        }

        public void Add(Destination dest)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = @"INSERT INTO Destinations (Name, Country, Region, Description, CreatedAt, UpdatedAt, IsDeleted) 
                        VALUES (@Name, @Country, @Region, @Desc, @Created, @Updated, 0)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Name", dest.Name);
            cmd.Parameters.AddWithValue("@Country", dest.Country);
            cmd.Parameters.AddWithValue("@Region", (object?)dest.Region ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Desc", (object?)dest.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Created", dest.CreatedAt == default ? DateTime.Now : dest.CreatedAt);
            cmd.Parameters.AddWithValue("@Updated", dest.UpdatedAt == default ? DateTime.Now : dest.UpdatedAt);
            cmd.ExecuteNonQuery();
        }

        public void Update(Destination dest)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = @"UPDATE Destinations SET Name=@Name, Country=@Country, Region=@Region, 
                        Description=@Desc, UpdatedAt=@Updated
                        WHERE DestinationId=@Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", dest.DestinationId);
            cmd.Parameters.AddWithValue("@Name", dest.Name);
            cmd.Parameters.AddWithValue("@Country", dest.Country);
            cmd.Parameters.AddWithValue("@Region", (object?)dest.Region ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Desc", (object?)dest.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Updated", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "UPDATE Destinations SET IsDeleted = 1, UpdatedAt = @Updated WHERE DestinationId = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Updated", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        public List<Destination> GetAll()
        {
            var list = new List<Destination>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "SELECT * FROM Destinations WHERE IsDeleted = 0 ORDER BY Name ASC";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapToDestination(reader));
            return list;
        }

        public Destination? GetById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "SELECT * FROM Destinations WHERE DestinationId = @Id AND IsDeleted = 0";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapToDestination(reader) : null;
        }

        public List<Destination> Search(string? name, string? country)
        {
            var list = new List<Destination>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "SELECT * FROM Destinations WHERE IsDeleted = 0";
            if (!string.IsNullOrEmpty(name)) sql += " AND Name LIKE @Name";
            if (!string.IsNullOrEmpty(country)) sql += " AND Country LIKE @Country";
            sql += " ORDER BY Name ASC";

            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrEmpty(name)) cmd.Parameters.AddWithValue("@Name", $"%{name}%");
            if (!string.IsNullOrEmpty(country)) cmd.Parameters.AddWithValue("@Country", $"%{country}%");

            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapToDestination(reader));
            return list;
        }

        public bool ExistsInTours(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "SELECT COUNT(1) FROM TourTemplates WHERE DestinationId = @Id AND IsDeleted = 0";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            return (int)cmd.ExecuteScalar() > 0;
        }

        public bool ExistsByName(string name, int? excludeId = null)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "SELECT COUNT(1) FROM Destinations WHERE Name = @Name";
            if (excludeId.HasValue) sql += " AND DestinationId != @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Name", name);
            if (excludeId.HasValue) cmd.Parameters.AddWithValue("@Id", excludeId.Value);
            return (int)cmd.ExecuteScalar() > 0;
        }

        private Destination MapToDestination(SqlDataReader reader)
        {
            return new Destination
            {
                DestinationId = (int)reader["DestinationId"],
                Name = reader["Name"].ToString() ?? "",
                Country = reader["Country"].ToString() ?? "",
                Region = reader["Region"] != DBNull.Value ? reader["Region"].ToString() : null,
                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null,
                CreatedAt = (DateTime)reader["CreatedAt"],
                UpdatedAt = (DateTime)reader["UpdatedAt"],
                IsDeleted = (bool)reader["IsDeleted"]
            };
        }
    }
}
