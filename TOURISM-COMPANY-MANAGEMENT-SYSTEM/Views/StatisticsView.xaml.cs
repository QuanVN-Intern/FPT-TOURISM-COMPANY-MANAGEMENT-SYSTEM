using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.BLL;
using TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Views
{
    public partial class StatisticsView : UserControl
    {
        private readonly StatisticsService _statisticsService;
        private DataTable? _currentTable;

        public StatisticsView()
        {
            InitializeComponent();
            _statisticsService = new StatisticsService();
            InitFilters();
            LoadCurrentSelection();
        }

        private void InitFilters()
        {
            CboReportType.ItemsSource = new List<string> { "Revenue", "Booking", "Tour", "Customer" };
            CboReportType.SelectedIndex = 0;
            DpFrom.SelectedDate = DateTime.Today.AddMonths(-6);
            DpTo.SelectedDate = DateTime.Today;
            LoadViewModes();
        }

        private void CboReportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadViewModes();
        }

        private void LoadViewModes()
        {
            var reportType = CboReportType.SelectedItem?.ToString() ?? "Revenue";
            List<string> modes = reportType switch
            {
                "Revenue" => new List<string> { "By Month", "By Tour" },
                "Booking" => new List<string> { "By Status", "By Time" },
                "Tour" => new List<string> { "Best Selling", "Least Booked", "Occupancy" },
                "Customer" => new List<string> { "By Bookings", "By Spending" },
                _ => new List<string> { "By Month" }
            };

            CboViewMode.ItemsSource = modes;
            CboViewMode.SelectedIndex = 0;
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadCurrentSelection();
        }

        private void LoadCurrentSelection()
        {
            try
            {
                var from = DpFrom.SelectedDate ?? DateTime.Today.AddMonths(-6);
                var to = DpTo.SelectedDate ?? DateTime.Today;
                var reportType = CboReportType.SelectedItem?.ToString() ?? "Revenue";
                var viewMode = CboViewMode.SelectedItem?.ToString() ?? "By Month";

                var dataset = _statisticsService.GetDataset(reportType, viewMode, from, to);
                _currentTable = dataset.Table;

                DataGridStatistics.ItemsSource = dataset.Table.DefaultView;
                ApplySummaryCards(dataset.SummaryCards, from, to);
                RenderChart(dataset.ChartTitle, dataset.ChartKind, dataset.PrimaryChartPoints);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load statistics: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RenderChart(string title, string kind, List<ChartPointDto> points)
        {
            var plot = PrimaryPlot;
            var model = new PlotModel { Title = title };
            TxtPrimaryChartTitle.Text = title;

            if (kind == "Pie")
            {
                var pieSeries = new PieSeries
                {
                    StrokeThickness = 1.0,
                    InsideLabelPosition = 0.8,
                    AngleSpan = 360,
                    StartAngle = 0
                };
                foreach (var p in points)
                {
                    pieSeries.Slices.Add(new PieSlice(p.Label, p.Value));
                }
                model.Series.Add(pieSeries);
                plot.Model = model;
                return;
            }

            var categoryAxis = new CategoryAxis { Position = AxisPosition.Bottom };
            foreach (var p in points) categoryAxis.Labels.Add(p.Label);
            model.Axes.Add(categoryAxis);
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left });

            if (kind == "Line")
            {
                var lineSeries = new LineSeries { Title = "Value", MarkerType = MarkerType.Circle };
                for (int i = 0; i < points.Count; i++) lineSeries.Points.Add(new DataPoint(i, points[i].Value));
                model.Series.Add(lineSeries);
            }
            else
            {
                var barSeries = new BarSeries { Title = "Value" };
                model.Axes.Clear();
                var yAxis = new CategoryAxis { Position = AxisPosition.Left };
                foreach (var p in points) yAxis.Labels.Add(p.Label);
                model.Axes.Add(yAxis);
                model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
                foreach (var p in points) barSeries.Items.Add(new BarItem(p.Value));
                model.Series.Add(barSeries);
            }

            plot.Model = model;
        }

        private void ApplySummaryCards(List<SummaryCard> cards, DateTime from, DateTime to)
        {
            TxtCard1Title.Text = cards.ElementAtOrDefault(0)?.Title ?? "Card 1";
            TxtCard1Value.Text = cards.ElementAtOrDefault(0)?.Value ?? "-";
            TxtCard2Title.Text = cards.ElementAtOrDefault(1)?.Title ?? "Card 2";
            TxtCard2Value.Text = cards.ElementAtOrDefault(1)?.Value ?? "-";
            TxtCard3Title.Text = "Date Range";
            TxtCard3Value.Text = $"{from:dd/MM/yyyy} - {to:dd/MM/yyyy}";
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentTable == null || _currentTable.Rows.Count == 0)
                {
                    MessageBox.Show("No data to export.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dlg = new SaveFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                    FileName = $"statistics-{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                };

                if (dlg.ShowDialog() == true)
                {
                    _statisticsService.ExportToExcel(_currentTable, dlg.FileName);
                    MessageBox.Show("Export successful.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
