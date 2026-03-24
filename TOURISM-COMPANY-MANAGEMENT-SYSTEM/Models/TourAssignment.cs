using System;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

public partial class TourAssignment
{
    public int AssignmentId { get; set; }
    public int TourId { get; set; }
    public int AccountId { get; set; }
    public int VehicleId { get; set; }

    // Navigation
    public virtual Tour Tour { get; set; } = null!;
    public virtual Account Account { get; set; } = null!;
    public virtual Vehicle Vehicle { get; set; } = null!;

    // Display helpers (Not mapped if needed, or just used in UI)
    public string DriverName => Account?.FullName ?? "Unknown";
    public string TourName => Tour?.TourName ?? "Unknown";
    public string VehiclePlate => Vehicle?.PlateNumber ?? "Unknown";
}
