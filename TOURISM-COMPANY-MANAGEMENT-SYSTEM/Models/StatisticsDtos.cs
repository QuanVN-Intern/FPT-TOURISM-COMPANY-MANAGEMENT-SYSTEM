using System.Data;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models
{
    public class SummaryCard
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class ChartPointDto
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
    }

    public class StatisticsDataset
    {
        public string ChartTitle { get; set; } = string.Empty;
        public string ChartKind { get; set; } = "Column";
        public List<ChartPointDto> PrimaryChartPoints { get; set; } = new();
        public string? SecondaryChartTitle { get; set; }
        public string? SecondaryChartKind { get; set; }
        public List<ChartPointDto> SecondaryChartPoints { get; set; } = new();
        public List<SummaryCard> SummaryCards { get; set; } = new();
        public DataTable Table { get; set; } = new();
    }
}
