using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL
{
    public class StatisticsRepository
    {
        private readonly string _connectionString;

        public StatisticsRepository()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnectionString")
                ?? "Server=DESKTOP-E9VL67H;database=TravelCompanyDB;uid=sa;pwd=123456;TrustServerCertificate=True;";
        }

        private SqlConnection OpenConnection()
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        private static DataTable ReadTable(SqlCommand cmd)
        {
            var dt = new DataTable();
            using var reader = cmd.ExecuteReader();
            dt.Load(reader);
            return dt;
        }

        public DataTable GetRevenueByMonth(DateTime from, DateTime to)
        {
            using var conn = OpenConnection();
            using var cmd = new SqlCommand(@"
SELECT FORMAT(B.BookingDate, 'yyyy-MM') AS [Month],
       SUM(B.TotalAmount) AS Revenue
FROM Bookings B
WHERE B.IsDeleted = 0
  AND B.BookingDate >= @FromDate
  AND B.BookingDate < DATEADD(day, 1, @ToDate)
GROUP BY FORMAT(B.BookingDate, 'yyyy-MM')
ORDER BY [Month];", conn);
            cmd.Parameters.AddWithValue("@FromDate", from.Date);
            cmd.Parameters.AddWithValue("@ToDate", to.Date);
            return ReadTable(cmd);
        }

        public DataTable GetRevenueByTour(DateTime from, DateTime to)
        {
            using var conn = OpenConnection();
            using var cmd = new SqlCommand(@"
SELECT T.TourName,
       SUM(B.TotalAmount) AS Revenue
FROM Bookings B
JOIN TourSchedules TS ON TS.ScheduleId = B.ScheduleId
JOIN TourTemplates T ON T.TourTemplateId = TS.TourTemplateId
WHERE B.IsDeleted = 0
  AND T.IsDeleted = 0
  AND B.BookingDate >= @FromDate
  AND B.BookingDate < DATEADD(day, 1, @ToDate)
GROUP BY T.TourName
ORDER BY Revenue DESC;", conn);
            cmd.Parameters.AddWithValue("@FromDate", from.Date);
            cmd.Parameters.AddWithValue("@ToDate", to.Date);
            return ReadTable(cmd);
        }

        public DataTable GetBookingByStatus(DateTime from, DateTime to)
        {
            using var conn = OpenConnection();
            using var cmd = new SqlCommand(@"
SELECT B.Status,
       COUNT(1) AS [Total Bookings]
FROM Bookings B
WHERE B.IsDeleted = 0
  AND B.BookingDate >= @FromDate
  AND B.BookingDate < DATEADD(day, 1, @ToDate)
GROUP BY B.Status
ORDER BY [Total Bookings] DESC;", conn);
            cmd.Parameters.AddWithValue("@FromDate", from.Date);
            cmd.Parameters.AddWithValue("@ToDate", to.Date);
            return ReadTable(cmd);
        }

        public DataTable GetBookingByMonth(DateTime from, DateTime to)
        {
            using var conn = OpenConnection();
            using var cmd = new SqlCommand(@"
SELECT FORMAT(B.BookingDate, 'yyyy-MM') AS [Month],
       COUNT(1) AS [Total Bookings]
FROM Bookings B
WHERE B.IsDeleted = 0
  AND B.BookingDate >= @FromDate
  AND B.BookingDate < DATEADD(day, 1, @ToDate)
GROUP BY FORMAT(B.BookingDate, 'yyyy-MM')
ORDER BY [Month];", conn);
            cmd.Parameters.AddWithValue("@FromDate", from.Date);
            cmd.Parameters.AddWithValue("@ToDate", to.Date);
            return ReadTable(cmd);
        }

        public DataTable GetTopBestSellingTours(DateTime from, DateTime to)
        {
            using var conn = OpenConnection();
            using var cmd = new SqlCommand(@"
SELECT TOP 10
       T.TourName,
       SUM(B.NumPersons) AS [Total Customers],
       CAST(SUM(B.NumPersons) * 100.0 / NULLIF(MAX(T.MaxCapacity), 0) AS decimal(6,2)) AS [Occupancy Rate]
FROM Bookings B
JOIN TourSchedules TS ON TS.ScheduleId = B.ScheduleId
JOIN TourTemplates T ON T.TourTemplateId = TS.TourTemplateId
WHERE B.IsDeleted = 0
  AND T.IsDeleted = 0
  AND B.Status <> 'Cancelled'
  AND B.BookingDate >= @FromDate
  AND B.BookingDate < DATEADD(day, 1, @ToDate)
GROUP BY T.TourName
ORDER BY [Total Customers] DESC;", conn);
            cmd.Parameters.AddWithValue("@FromDate", from.Date);
            cmd.Parameters.AddWithValue("@ToDate", to.Date);
            return ReadTable(cmd);
        }

        public DataTable GetLeastBookedTours(DateTime from, DateTime to)
        {
            using var conn = OpenConnection();
            using var cmd = new SqlCommand(@"
SELECT TOP 10
       T.TourName,
       ISNULL(SUM(CASE WHEN B.Status <> 'Cancelled' THEN B.NumPersons ELSE 0 END), 0) AS [Total Customers],
       CAST(ISNULL(SUM(CASE WHEN B.Status <> 'Cancelled' THEN B.NumPersons ELSE 0 END), 0) * 100.0 / NULLIF(MAX(T.MaxCapacity), 0) AS decimal(6,2)) AS [Occupancy Rate]
FROM TourTemplates T
LEFT JOIN TourSchedules TS ON T.TourTemplateId = TS.TourTemplateId AND TS.IsDeleted = 0
LEFT JOIN Bookings B ON B.ScheduleId = TS.ScheduleId
    AND B.IsDeleted = 0
    AND B.BookingDate >= @FromDate
    AND B.BookingDate < DATEADD(day, 1, @ToDate)
WHERE T.IsDeleted = 0
GROUP BY T.TourName
ORDER BY [Total Customers] ASC;", conn);
            cmd.Parameters.AddWithValue("@FromDate", from.Date);
            cmd.Parameters.AddWithValue("@ToDate", to.Date);
            return ReadTable(cmd);
        }

        public DataTable GetTourOccupancy(DateTime from, DateTime to)
        {
            using var conn = OpenConnection();
            using var cmd = new SqlCommand(@"
SELECT T.TourName,
       ISNULL(SUM(CASE WHEN B.Status <> 'Cancelled' THEN B.NumPersons ELSE 0 END), 0) AS [Total Customers],
       CAST(ISNULL(SUM(CASE WHEN B.Status <> 'Cancelled' THEN B.NumPersons ELSE 0 END), 0) * 100.0 / NULLIF(MAX(T.MaxCapacity), 0) AS decimal(6,2)) AS [Occupancy Rate]
FROM TourTemplates T
LEFT JOIN TourSchedules TS ON T.TourTemplateId = TS.TourTemplateId AND TS.IsDeleted = 0
LEFT JOIN Bookings B ON B.ScheduleId = TS.ScheduleId
    AND B.IsDeleted = 0
    AND B.BookingDate >= @FromDate
    AND B.BookingDate < DATEADD(day, 1, @ToDate)
WHERE T.IsDeleted = 0
GROUP BY T.TourName
ORDER BY [Occupancy Rate] DESC;", conn);
            cmd.Parameters.AddWithValue("@FromDate", from.Date);
            cmd.Parameters.AddWithValue("@ToDate", to.Date);
            return ReadTable(cmd);
        }

        public DataTable GetTopCustomersByBookings(DateTime from, DateTime to)
        {
            using var conn = OpenConnection();
            using var cmd = new SqlCommand(@"
SELECT TOP 10
       C.FullName AS CustomerName,
       COUNT(B.BookingId) AS [Total Bookings],
       SUM(B.TotalAmount) AS [Total Spent]
FROM Customers C
JOIN Bookings B ON B.CustomerId = C.CustomerId
WHERE C.IsDeleted = 0
  AND B.IsDeleted = 0
  AND B.BookingDate >= @FromDate
  AND B.BookingDate < DATEADD(day, 1, @ToDate)
GROUP BY C.FullName
ORDER BY [Total Bookings] DESC;", conn);
            cmd.Parameters.AddWithValue("@FromDate", from.Date);
            cmd.Parameters.AddWithValue("@ToDate", to.Date);
            return ReadTable(cmd);
        }

        public DataTable GetTopCustomersBySpending(DateTime from, DateTime to)
        {
            using var conn = OpenConnection();
            using var cmd = new SqlCommand(@"
SELECT TOP 10
       C.FullName AS CustomerName,
       COUNT(B.BookingId) AS [Total Bookings],
       SUM(B.TotalAmount) AS [Total Spent]
FROM Customers C
JOIN Bookings B ON B.CustomerId = C.CustomerId
WHERE C.IsDeleted = 0
  AND B.IsDeleted = 0
  AND B.BookingDate >= @FromDate
  AND B.BookingDate < DATEADD(day, 1, @ToDate)
GROUP BY C.FullName
ORDER BY [Total Spent] DESC;", conn);
            cmd.Parameters.AddWithValue("@FromDate", from.Date);
            cmd.Parameters.AddWithValue("@ToDate", to.Date);
            return ReadTable(cmd);
        }

        public decimal GetTotalRevenue(DateTime from, DateTime to)
        {
            using var conn = OpenConnection();
            using var cmd = new SqlCommand(@"
SELECT ISNULL(SUM(B.TotalAmount), 0)
FROM Bookings B
WHERE B.IsDeleted = 0
  AND B.BookingDate >= @FromDate
  AND B.BookingDate < DATEADD(day, 1, @ToDate);", conn);
            cmd.Parameters.AddWithValue("@FromDate", from.Date);
            cmd.Parameters.AddWithValue("@ToDate", to.Date);
            return Convert.ToDecimal(cmd.ExecuteScalar() ?? 0m);
        }

        public int GetTotalBookings(DateTime from, DateTime to)
        {
            using var conn = OpenConnection();
            using var cmd = new SqlCommand(@"
SELECT COUNT(1)
FROM Bookings B
WHERE B.IsDeleted = 0
  AND B.BookingDate >= @FromDate
  AND B.BookingDate < DATEADD(day, 1, @ToDate);", conn);
            cmd.Parameters.AddWithValue("@FromDate", from.Date);
            cmd.Parameters.AddWithValue("@ToDate", to.Date);
            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }

        public int GetTotalCustomers()
        {
            using var conn = OpenConnection();
            using var cmd = new SqlCommand("SELECT COUNT(1) FROM Customers WHERE IsDeleted = 0;", conn);
            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }
    }
}
