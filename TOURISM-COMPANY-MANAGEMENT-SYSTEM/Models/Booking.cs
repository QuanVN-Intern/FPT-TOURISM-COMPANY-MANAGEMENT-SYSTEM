using System;
using System.Collections.Generic;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public string BookingCode { get; set; } = null!;

    public int CustomerId { get; set; }

    public int TourId { get; set; }

    public int AccountId { get; set; }

    public int NumPersons { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime BookingDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CancelledAt { get; set; }

    public string? CancelReason { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Tour Tour { get; set; } = null!;
}
