using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.DAL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL
{
    public class StatisticsService
    {
        private readonly StatisticsRepository _repository;

        public StatisticsService()
        {
            _repository = new StatisticsRepository();
        }

        public StatisticsDataset GetDataset(string reportType, string viewMode, DateTime from, DateTime to)
        {
            if (from.Date > to.Date)
                throw new Exception("From Date cannot be greater than To Date.");

            return reportType switch
            {
                "Revenue" => BuildRevenueDataset(viewMode, from, to),
                "Booking" => BuildBookingDataset(viewMode, from, to),
                "Tour" => BuildTourDataset(viewMode, from, to),
                "Customer" => BuildCustomerDataset(viewMode, from, to),
                _ => throw new Exception("Invalid report type.")
            };
        }

        public void ExportToExcel(DataTable table, string filePath)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(table, "Statistics");
            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }

        private StatisticsDataset BuildRevenueDataset(string viewMode, DateTime from, DateTime to)
        {
            var table = viewMode == "By Tour"
                ? _repository.GetRevenueByTour(from, to)
                : _repository.GetRevenueByMonth(from, to);

            var cards = new List<SummaryCard>
            {
                new SummaryCard { Title = "Total Revenue", Value = _repository.GetTotalRevenue(from, to).ToString("C0", CultureInfo.CurrentCulture) },
                new SummaryCard { Title = "Rows", Value = table.Rows.Count.ToString() }
            };

            return new StatisticsDataset
            {
                ChartTitle = $"Revenue {viewMode}",
                ChartKind = viewMode == "By Tour" ? "Column" : "Line",
                SummaryCards = cards,
                Table = table,
                PrimaryChartPoints = BuildPoints(table, table.Columns[0].ColumnName, "Revenue")
            };
        }

        private StatisticsDataset BuildBookingDataset(string viewMode, DateTime from, DateTime to)
        {
            var statusTable = _repository.GetBookingByStatus(from, to);
            var timeTable = _repository.GetBookingByMonth(from, to);
            var cards = new List<SummaryCard>
            {
                new SummaryCard { Title = "Total Bookings", Value = _repository.GetTotalBookings(from, to).ToString() },
                new SummaryCard { Title = "Status Types", Value = statusTable.Rows.Count.ToString() }
            };

            if (viewMode == "By Time")
            {
                return new StatisticsDataset
                {
                    ChartTitle = "Bookings by Time",
                    ChartKind = "Line",
                    SecondaryChartTitle = "Bookings by Status",
                    SecondaryChartKind = "Pie",
                    SummaryCards = cards,
                    Table = timeTable,
                    PrimaryChartPoints = BuildPoints(timeTable, "Month", "Total Bookings"),
                    SecondaryChartPoints = BuildPoints(statusTable, "Status", "Total Bookings")
                };
            }

            return new StatisticsDataset
            {
                ChartTitle = "Bookings by Status",
                ChartKind = "Pie",
                SecondaryChartTitle = "Bookings by Time",
                SecondaryChartKind = "Line",
                SummaryCards = cards,
                Table = statusTable,
                PrimaryChartPoints = BuildPoints(statusTable, "Status", "Total Bookings"),
                SecondaryChartPoints = BuildPoints(timeTable, "Month", "Total Bookings")
            };
        }

        private StatisticsDataset BuildTourDataset(string viewMode, DateTime from, DateTime to)
        {
            var table = viewMode switch
            {
                "Least Booked" => _repository.GetLeastBookedTours(from, to),
                "Occupancy" => _repository.GetTourOccupancy(from, to),
                _ => _repository.GetTopBestSellingTours(from, to)
            };

            return new StatisticsDataset
            {
                ChartTitle = $"Tour Statistics - {viewMode}",
                ChartKind = "Column",
                SummaryCards = new List<SummaryCard>
                {
                    new SummaryCard { Title = "Tours In Table", Value = table.Rows.Count.ToString() },
                    new SummaryCard { Title = "Top Occupancy", Value = GetMaxValue(table, "Occupancy Rate").ToString("0.##") + "%" }
                },
                Table = table,
                PrimaryChartPoints = BuildPoints(table, "TourName", "Total Customers")
            };
        }

        private StatisticsDataset BuildCustomerDataset(string viewMode, DateTime from, DateTime to)
        {
            var table = viewMode == "By Spending"
                ? _repository.GetTopCustomersBySpending(from, to)
                : _repository.GetTopCustomersByBookings(from, to);

            return new StatisticsDataset
            {
                ChartTitle = $"Customer Statistics - {viewMode}",
                ChartKind = "Column",
                SummaryCards = new List<SummaryCard>
                {
                    new SummaryCard { Title = "Total Customers", Value = _repository.GetTotalCustomers().ToString() },
                    new SummaryCard { Title = "Top 10 Rows", Value = table.Rows.Count.ToString() }
                },
                Table = table,
                PrimaryChartPoints = BuildPoints(table, "CustomerName", viewMode == "By Spending" ? "Total Spent" : "Total Bookings")
            };
        }

        private static List<ChartPointDto> BuildPoints(DataTable table, string labelCol, string valueCol)
        {
            var points = new List<ChartPointDto>();
            foreach (DataRow row in table.Rows)
            {
                points.Add(new ChartPointDto
                {
                    Label = row[labelCol]?.ToString() ?? string.Empty,
                    Value = Convert.ToDouble(row[valueCol] == DBNull.Value ? 0 : row[valueCol])
                });
            }
            return points;
        }

        private static decimal GetMaxValue(DataTable table, string colName)
        {
            decimal max = 0;
            foreach (DataRow row in table.Rows)
            {
                var value = Convert.ToDecimal(row[colName] == DBNull.Value ? 0 : row[colName]);
                if (value > max) max = value;
            }
            return max;
        }
    }
}
