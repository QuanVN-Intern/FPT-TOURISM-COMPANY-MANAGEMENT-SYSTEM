using System;
using System.Collections.Generic;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string? PassportNo { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    // Navigation
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}