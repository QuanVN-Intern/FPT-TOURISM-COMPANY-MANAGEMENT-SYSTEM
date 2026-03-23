using System;
using System.Collections.Generic;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

public partial class Destination
{
    public int DestinationId { get; set; }

    public string Name { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string? Region { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
