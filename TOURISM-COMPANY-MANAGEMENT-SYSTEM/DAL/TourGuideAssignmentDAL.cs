using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL
{
    public class TourGuideAssignmentDAL
    {
        private readonly string _conn;

        public TourGuideAssignmentDAL()
        {
            var cfg = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true).Build();
            _conn = cfg.GetConnectionString("DefaultConnectionString")
                ?? "Server=localhost\\SQLEXPRESS;database=TravelCompanyDB;uid=sa;pwd=123456;TrustServerCertificate=True;";
        }

        public List<TourGuideAssignment> GetAll()
        {
            var list = new List<TourGuideAssignment>();
            using var conn = new SqlConnection(_conn);
            conn.Open();
            var sql = @"
                SELECT ga.*, tt.TourName, ts.DepartureDate, a.FullName AS GuideName
                FROM   TourGuideAssignments ga
                JOIN   TourSchedules  ts ON ga.ScheduleId = ts.ScheduleId
                JOIN   TourTemplates  tt ON ts.TourTemplateId = tt.TourTemplateId
                JOIN   Accounts        a ON ga.AccountId  = a.AccountId
                WHERE  ts.IsDeleted = 0
                ORDER BY ts.DepartureDate";
            using var cmd = new SqlCommand(sql, conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new TourGuideAssignment
                {
                    GuideAssignmentId = (int)r["GuideAssignmentId"],
                    ScheduleId = (int)r["ScheduleId"],
                    AccountId  = (int)r["AccountId"],
                    TourSchedule = new TourSchedule
                    {
                        DepartureDate = DateOnly.FromDateTime((DateTime)r["DepartureDate"]),
                        TourTemplate = new TourTemplate { TourName = r["TourName"].ToString() ?? "" }
                    },
                    Account = new Account { FullName = r["GuideName"].ToString() ?? "" }
                });
            }
            return list;
        }

        public void Add(int scheduleId, int accountId)
        {
            using var conn = new SqlConnection(_conn);
            conn.Open();
            var sql = "INSERT INTO TourGuideAssignments (ScheduleId, AccountId) VALUES (@S, @A)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@S", scheduleId);
            cmd.Parameters.AddWithValue("@A", accountId);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SqlConnection(_conn);
            conn.Open();
            using var cmd = new SqlCommand("DELETE FROM TourGuideAssignments WHERE GuideAssignmentId=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        public bool IsDuplicate(int scheduleId, int accountId)
        {
            using var conn = new SqlConnection(_conn);
            conn.Open();
            var sql = "SELECT COUNT(1) FROM TourGuideAssignments WHERE ScheduleId=@S AND AccountId=@A";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@S", scheduleId);
            cmd.Parameters.AddWithValue("@A", accountId);
            return (int)cmd.ExecuteScalar() > 0;
        }

        /// <summary>Check if guide is already on another overlapping schedule.</summary>
        public bool HasOverlappingSchedule(int accountId, DateOnly departure, DateOnly returnDate, int excludeScheduleId = 0)
        {
            using var conn = new SqlConnection(_conn);
            conn.Open();
            var sql = @"
                SELECT COUNT(1)
                FROM   TourGuideAssignments ga
                JOIN   TourSchedules ts ON ga.ScheduleId = ts.ScheduleId
                WHERE  ga.AccountId = @A
                  AND  ga.ScheduleId <> @Excl
                  AND  ts.IsDeleted  = 0
                  AND  ts.DepartureDate <= @Return
                  AND  ts.ReturnDate   >= @Depart";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@A",      accountId);
            cmd.Parameters.AddWithValue("@Excl",   excludeScheduleId);
            cmd.Parameters.AddWithValue("@Depart", departure.ToDateTime(TimeOnly.MinValue));
            cmd.Parameters.AddWithValue("@Return", returnDate.ToDateTime(TimeOnly.MinValue));
            return (int)cmd.ExecuteScalar() > 0;
        }
    }
}
