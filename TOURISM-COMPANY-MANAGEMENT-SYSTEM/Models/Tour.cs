using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

public partial class Tour
{
    public int TourId { get; set; }
    public string TourCode { get; set; } = null!;
    public string TourName { get; set; } = null!;
    public int DestinationId { get; set; }

    [NotMapped]
    public string? DestinationName { get; set; }
    public int DurationDays { get; set; }
    public decimal PricePerPerson { get; set; }
    public int MaxCapacity { get; set; }
    public int AvailableSlots { get; set; }
    public int BookedSlots => MaxCapacity - AvailableSlots;
    public DateOnly DepartureDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual Destination Destination { get; set; } = null!;
}
