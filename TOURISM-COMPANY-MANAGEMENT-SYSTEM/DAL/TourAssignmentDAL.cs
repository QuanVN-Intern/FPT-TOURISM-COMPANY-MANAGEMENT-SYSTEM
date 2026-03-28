using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL
{
    public class TourAssignmentDAL
    {
        private readonly string _connectionString;

        public TourAssignmentDAL()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
            _connectionString = config.GetConnectionString("DefaultConnectionString")
                ?? "Server=localhost\\SQLEXPRESS;database=TravelCompanyDB;uid=sa;pwd=123456;TrustServerCertificate=True;";
        }

        public List<TourAssignment> GetAll()
        {
            var list = new List<TourAssignment>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = @"SELECT ta.*, tt.TourName, a.FullName as DriverName, v.PlateNumber
                        FROM   TourAssignments ta
                        JOIN   TourSchedules   t ON ta.ScheduleId = t.ScheduleId
                        JOIN   TourTemplates   tt ON t.TourTemplateId = tt.TourTemplateId
                        JOIN   Accounts        a ON ta.AccountId = a.AccountId
                        JOIN   Vehicles        v ON ta.VehicleId = v.VehicleId";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new TourAssignment
                {
                    AssignmentId = (int)reader["AssignmentId"],
                    ScheduleId = (int)reader["ScheduleId"],
                    AccountId = (int)reader["AccountId"],
                    VehicleId = (int)reader["VehicleId"],
                    TourSchedule = new TourSchedule { TourTemplate = new TourTemplate { TourName = reader["TourName"].ToString() ?? "" } },
                    Account = new Account { FullName = reader["DriverName"].ToString() ?? "" },
                    Vehicle = new Vehicle { PlateNumber = reader["PlateNumber"].ToString() ?? "" }
                });
            }
            return list;
        }

        public void Add(TourAssignment assignment)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = @"INSERT INTO TourAssignments (ScheduleId, AccountId, VehicleId)
                        VALUES (@ScheduleId, @AccountId, @VehicleId)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ScheduleId", assignment.ScheduleId);
            cmd.Parameters.AddWithValue("@AccountId", assignment.AccountId);
            cmd.Parameters.AddWithValue("@VehicleId", assignment.VehicleId);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "DELETE FROM TourAssignments WHERE AssignmentId = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        public bool IsDuplicate(int scheduleId, int accountId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = "SELECT COUNT(1) FROM TourAssignments WHERE ScheduleId = @ScheduleId AND AccountId = @AccountId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ScheduleId", scheduleId);
            cmd.Parameters.AddWithValue("@AccountId", accountId);
            return (int)cmd.ExecuteScalar() > 0;
        }
    }
}
