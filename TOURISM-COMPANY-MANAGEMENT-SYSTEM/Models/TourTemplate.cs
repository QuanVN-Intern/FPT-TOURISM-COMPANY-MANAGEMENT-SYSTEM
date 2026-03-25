using System;
using System.Collections.Generic;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

public partial class TourTemplate
{
    public int TourTemplateId { get; set; }
    public string TourCode { get; set; } = null!;
    public string TourName { get; set; } = null!;
    public int DestinationId { get; set; }

    public int DurationDays { get; set; }
    public decimal PricePerPerson { get; set; }
    public int MaxCapacity { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public virtual Destination Destination { get; set; } = null!;
    public virtual ICollection<TourSchedule> TourSchedules { get; set; } = new List<TourSchedule>();

    public string DestinationName => Destination?.Name ?? "Unknown";
}
