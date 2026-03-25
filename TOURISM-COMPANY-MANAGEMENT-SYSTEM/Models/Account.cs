using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int RoleId { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    
    public string? LicenseNumber { get; set; }

    // Navigation
    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<TourAssignment> TourAssignments { get; set; } = new List<TourAssignment>();

    /// <summary>Populated by JOIN in AccountRepository — not a DB column.</summary>
    [NotMapped]
    public string RoleName { get; set; } = string.Empty;
}