using System;
using System.Collections.Generic;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

public partial class TourSchedule
{
    public int ScheduleId { get; set; }
    public int TourTemplateId { get; set; }
    public DateOnly DepartureDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public int AvailableSlots { get; set; }
    public string Status { get; set; } = "Active";
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public virtual TourTemplate TourTemplate { get; set; } = null!;
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<TourAssignment> TourAssignments { get; set; } = new List<TourAssignment>();
    public virtual ICollection<TourVehicle> TourVehicles { get; set; } = new List<TourVehicle>();

    public string DisplayText => $"{TourTemplate?.TourName} ({DepartureDate:dd/MM/yyyy})";
}
