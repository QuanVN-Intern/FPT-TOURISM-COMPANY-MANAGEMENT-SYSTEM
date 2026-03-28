namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

public partial class TourGuideAssignment
{
    public int GuideAssignmentId { get; set; }
    public int ScheduleId { get; set; }
    public int AccountId  { get; set; }

    public virtual TourSchedule TourSchedule { get; set; } = null!;
    public virtual Account Account { get; set; } = null!;

    // Display helpers
    public string GuideName => Account?.FullName ?? "Unknown";
    public string TourName  => TourSchedule?.TourTemplate?.TourName ?? "Unknown";
    public string DepartureText => TourSchedule?.DepartureDate.ToString("dd/MM/yyyy") ?? "";
}
