using System;
using System.Collections.Generic;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

public partial class TourVehicle
{
    public int Id { get; set; }

    public int TourId { get; set; }

    public int ScheduleId { get; set; }

    public int VehicleId { get; set; }

    public virtual Vehicle Vehicle { get; set; } = null!;
    public virtual TourSchedule TourSchedule { get; set; } = null!;
}
