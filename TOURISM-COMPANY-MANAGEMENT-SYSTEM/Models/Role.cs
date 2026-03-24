using System;
using System.Collections.Generic;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    // Navigation
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}