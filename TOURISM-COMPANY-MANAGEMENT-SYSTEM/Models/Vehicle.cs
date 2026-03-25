using System;
using System.Collections.Generic;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

public partial class Vehicle
{
    public int VehicleId { get; set; }

    public string PlateNumber { get; set; } = null!;

    public int Capacity { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public int? DriverId { get; set; }
    public virtual Account? Driver { get; set; }

    public virtual ICollection<TourVehicle> TourVehicles { get; set; } = new List<TourVehicle>();
    public virtual ICollection<TourAssignment> TourAssignments { get; set; } = new List<TourAssignment>();
}
