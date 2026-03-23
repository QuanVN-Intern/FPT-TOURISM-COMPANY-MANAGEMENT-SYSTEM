using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL
{
    public class TourRepository
    {
        private readonly string _connectionString;

        public TourRepository()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnectionString") 
                ?? "Server=DESKTOP-E9VL67H;database=TravelCompanyDB;uid=sa;pwd=123456;TrustServerCertificate=True;";
        }

        public void Add(Tour tour)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            // Computed column ReturnDate removed from INSERT
            var sql = @"INSERT INTO Tours (TourCode, TourName, DestinationId, DurationDays, PricePerPerson, MaxCapacity, AvailableSlots, DepartureDate, Description, ThumbnailUrl, Status, CreatedAt, UpdatedAt, IsDeleted) 
                        VALUES (@Code, @Name, @DestId, @Days, @Price, @MaxCap, @Avail, @DepDate, @Desc, @Thumb, @Status, @Created, @Updated, 0)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Code", tour.TourCode ?? string.Empty);
            cmd.Parameters.AddWithValue("@Name", tour.TourName ?? string.Empty);
            cmd.Parameters.AddWithValue("@DestId", tour.DestinationId);
            cmd.Parameters.AddWithValue("@Days", tour.DurationDays);
            cmd.Parameters.AddWithValue("@Price", tour.PricePerPerson);
            cmd.Parameters.AddWithValue("@MaxCap", tour.MaxCapacity);
            cmd.Parameters.AddWithValue("@Avail", tour.AvailableSlots);
            
            // Convert DateOnly to DateTime for SQL Server
            cmd.Parameters.AddWithValue("@DepDate", tour.DepartureDate.ToDateTime(TimeOnly.MinValue));
            
            cmd.Parameters.AddWithValue("@Desc", string.IsNullOrEmpty(tour.Description) ? DBNull.Value : tour.Description);
            cmd.Parameters.AddWithValue("@Thumb", string.IsNullOrEmpty(tour.ThumbnailUrl) ? DBNull.Value : tour.ThumbnailUrl);
            cmd.Parameters.AddWithValue("@Status", tour.Status ?? "Active");
            cmd.Parameters.AddWithValue("@Created", tour.CreatedAt == default ? DateTime.Now : tour.CreatedAt);
            cmd.Parameters.AddWithValue("@Updated", tour.UpdatedAt == default ? DateTime.Now : tour.UpdatedAt);
            
            cmd.ExecuteNonQuery();
        }

        public void Update(Tour tour)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            // Computed column ReturnDate removed from UPDATE
            var sql = @"UPDATE Tours SET TourCode=@Code, TourName=@Name, DestinationId=@DestId, DurationDays=@Days, 
                        PricePerPerson=@Price, MaxCapacity=@MaxCap, AvailableSlots=@Avail, DepartureDate=@DepDate, 
                        Description=@Desc, ThumbnailUrl=@Thumb, Status=@Status, UpdatedAt=@Updated
                        WHERE TourId=@Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", tour.TourId);
            cmd.Parameters.AddWithValue("@Code", tour.TourCode ?? string.Empty);
            cmd.Parameters.AddWithValue("@Name", tour.TourName ?? string.Empty);
            cmd.Parameters.AddWithValue("@DestId", tour.DestinationId);
            cmd.Parameters.AddWithValue("@Days", tour.DurationDays);
            cmd.Parameters.AddWithValue("@Price", tour.PricePerPerson);
            cmd.Parameters.AddWithValue("@MaxCap", tour.MaxCapacity);
            cmd.Parameters.AddWithValue("@Avail", tour.AvailableSlots);

            cmd.Parameters.AddWithValue("@DepDate", tour.DepartureDate.ToDateTime(TimeOnly.MinValue));

            cmd.Parameters.AddWithValue("@Desc", string.IsNullOrEmpty(tour.Description) ? DBNull.Value : tour.Description);
            cmd.Parameters.AddWithValue("@Thumb", string.IsNullOrEmpty(tour.ThumbnailUrl) ? DBNull.Value : tour.ThumbnailUrl);
            cmd.Parameters.AddWithValue("@Status", tour.Status ?? "Active");
            cmd.Parameters.AddWithValue("@Updated", tour.UpdatedAt == default ? DateTime.Now : tour.UpdatedAt);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int tourId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "UPDATE Tours SET IsDeleted = 1, UpdatedAt = @Updated WHERE TourId = @Id"; // Soft Delete
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", tourId);
            cmd.Parameters.AddWithValue("@Updated", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        public Tour? GetById(int tourId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "SELECT * FROM Tours WHERE TourId = @Id AND IsDeleted = 0";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", tourId);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapToTour(reader) : null;
        }

        public List<Tour> GetAll()
        {
            var list = new List<Tour>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "SELECT * FROM Tours WHERE IsDeleted = 0 ORDER BY CreatedAt DESC";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapToTour(reader));
            return list;
        }

        public List<Tour> Search(string? name, int? destId, decimal? minPrice, decimal? maxPrice)
        {
            var list = new List<Tour>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            
            var sql = "SELECT * FROM Tours WHERE IsDeleted = 0";
            if (!string.IsNullOrEmpty(name)) sql += " AND TourName LIKE @Name";
            if (destId.HasValue) sql += " AND DestinationId = @DestId";
            if (minPrice.HasValue) sql += " AND PricePerPerson >= @MinPrice";
            if (maxPrice.HasValue) sql += " AND PricePerPerson <= @MaxPrice";
            sql += " ORDER BY CreatedAt DESC";

            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrEmpty(name)) cmd.Parameters.AddWithValue("@Name", $"%{name}%");
            if (destId.HasValue) cmd.Parameters.AddWithValue("@DestId", destId.Value);
            if (minPrice.HasValue) cmd.Parameters.AddWithValue("@MinPrice", minPrice.Value);
            if (maxPrice.HasValue) cmd.Parameters.AddWithValue("@MaxPrice", maxPrice.Value);

            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapToTour(reader));
            return list;
        }

        public bool ExistsByCode(string code, int? excludeId = null)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "SELECT COUNT(1) FROM Tours WHERE TourCode = @Code AND IsDeleted = 0";
            if (excludeId.HasValue) sql += " AND TourId != @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Code", code ?? string.Empty);
            if (excludeId.HasValue) cmd.Parameters.AddWithValue("@Id", excludeId.Value);
            return (int)cmd.ExecuteScalar() > 0;
        }

        public bool DestinationExists(int destId)
        {
            try 
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                var sql = "SELECT COUNT(1) FROM Destinations WHERE DestinationId = @Id";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", destId);
                return (int)cmd.ExecuteScalar() > 0;
            } 
            catch { return true; } // Bypass if table does not exist
        }

        public List<KeyValuePair<int, string>> GetDestinations()
        {
            var list = new List<KeyValuePair<int, string>>();
            try 
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                var sql = "SELECT DestinationId, Name FROM Destinations WHERE IsDeleted = 0";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new KeyValuePair<int, string>((int)reader["DestinationId"], reader["Name"].ToString() ?? ""));
                }
            } 
            catch { }
            return list;
        }

        public int GetTotalBookedSlots(int tourId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "SELECT SUM(NumberOfPeople) FROM Bookings WHERE TourId = @Id AND Status != 'Cancelled'";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", tourId);
            try
            {
                var result = cmd.ExecuteScalar();
                return result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
            catch { return 0; }
        }

        public bool HasBookings(int tourId) => GetTotalBookedSlots(tourId) > 0;

        private Tour MapToTour(SqlDataReader reader)
        {
            var t = new Tour
            {
                TourId = (int)reader["TourId"],
                TourCode = reader["TourCode"].ToString() ?? "",
                TourName = reader["TourName"].ToString() ?? "",
                DestinationId = (int)reader["DestinationId"],
                DurationDays = (int)reader["DurationDays"],
                PricePerPerson = (decimal)reader["PricePerPerson"],
                MaxCapacity = (int)reader["MaxCapacity"],
                AvailableSlots = (int)reader["AvailableSlots"],
                DepartureDate = DateOnly.FromDateTime((DateTime)reader["DepartureDate"]),
                Status = reader["Status"].ToString() ?? "Active",
            };

            if (reader["ReturnDate"] != DBNull.Value) 
                t.ReturnDate = DateOnly.FromDateTime((DateTime)reader["ReturnDate"]);
            
            if (reader["Description"] != DBNull.Value) 
                t.Description = reader["Description"].ToString();
            
            if (reader["ThumbnailUrl"] != DBNull.Value) 
                t.ThumbnailUrl = reader["ThumbnailUrl"].ToString();

            // CreatedAt and UpdatedAt might not exist in old DB structure, handle gracefully
            try 
            {
                t.CreatedAt = (DateTime)reader["CreatedAt"];
                t.UpdatedAt = (DateTime)reader["UpdatedAt"];
                t.IsDeleted = (bool)reader["IsDeleted"];
            } 
            catch { }

            return t;
        }
    }
}
