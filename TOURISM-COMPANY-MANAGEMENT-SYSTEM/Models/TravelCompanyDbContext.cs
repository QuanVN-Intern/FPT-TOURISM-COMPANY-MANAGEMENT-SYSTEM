using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace TOURISM_COMPANY_MANAGEMENT_SYSTEM.Models;

public partial class TravelCompanyDbContext : DbContext
{
    public TravelCompanyDbContext()
    {
    }

    public TravelCompanyDbContext(DbContextOptions<TravelCompanyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Destination> Destinations { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TourTemplate> TourTemplates { get; set; }
    public virtual DbSet<TourSchedule> TourSchedules { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }
    public virtual DbSet<TourVehicle> TourVehicles { get; set; }
    public virtual DbSet<TourAssignment> TourAssignments { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(GetConnectionString());
    }
    private String GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .Build();
        var strConn = config["ConnectionStrings:DefaultConnectionString"];

        return strConn;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Accounts__349DA5A61368F4C0");

            entity.HasIndex(e => e.Email, "IX_Accounts_Email");

            entity.HasIndex(e => e.RoleId, "IX_Accounts_RoleId");

            entity.HasIndex(e => e.Username, "UQ__Accounts__536C85E468F37E8B").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Accounts__A9D1053465BD69E8").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Acc_Role");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Bookings__73951AED109AE5C6");

            entity.HasIndex(e => e.CustomerId, "IX_Bookings_Customer");

            entity.HasIndex(e => e.BookingDate, "IX_Bookings_Date");

            entity.HasIndex(e => e.Status, "IX_Bookings_Status");

            entity.HasIndex(e => e.ScheduleId, "IX_Bookings_Schedule");

            entity.HasIndex(e => e.BookingCode, "UQ__Bookings__C6E56BD54FBD8E8F").IsUnique();

            entity.Property(e => e.BookingCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.BookingDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CancelReason).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Confirmed");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Account).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Book_Account");

            entity.HasOne(d => d.Customer).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Book_Customer");

            entity.HasOne(d => d.TourSchedule).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Book_Schedule");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64D84D62506D");

            entity.HasIndex(e => e.FullName, "IX_Customers_FullName");

            entity.HasIndex(e => e.Phone, "IX_Customers_Phone");

            entity.HasIndex(e => e.Phone, "UQ__Customer__5C7E359E4F8A31DB").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.PassportNo).HasMaxLength(50);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Destination>(entity =>
        {
            entity.HasKey(e => e.DestinationId).HasName("PK__Destinat__DB5FE4CC191F5711");

            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A3811EA2695");

            entity.HasIndex(e => e.BookingId, "IX_Payments_Booking");

            entity.HasIndex(e => e.PaymentDate, "IX_Payments_Date");

            entity.HasIndex(e => e.Status, "IX_Payments_Status");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasDefaultValue("Cash");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TransactionRef).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Booking).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pay_Booking");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__Permissi__EFA6FB2F3A1F26D1");

            entity.HasIndex(e => new { e.Module, e.Action }, "UQ_Perm_Module_Action").IsUnique();

            entity.HasIndex(e => e.PermissionName, "UQ__Permissi__0FFDA35747DB80E1").IsUnique();

            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Module).HasMaxLength(50);
            entity.Property(e => e.PermissionName).HasMaxLength(100);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A97C04911");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B6160D7987A6E").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RP_Perm"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RP_Role"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId").HasName("PK_RolePerm");
                        j.ToTable("RolePermissions");
                    });
        });

        modelBuilder.Entity<TourTemplate>(entity =>
        {
            entity.HasKey(e => e.TourTemplateId).HasName("PK__TourTemplates");

            entity.HasIndex(e => e.TourCode, "UQ__TourTemplates_Code").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PricePerPerson).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.TourCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TourName).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Destination).WithMany(p => p.TourTemplates)
                .HasForeignKey(d => d.DestinationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Template_Destination");
        });

        modelBuilder.Entity<TourSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__TourSchedules");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.TourTemplate).WithMany(p => p.TourSchedules)
                .HasForeignKey(d => d.TourTemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Schedule_Template");
        });

        modelBuilder.Entity<TourVehicle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourVehi__3214EC074B5E385F");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.TourVehicles)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TourVehic__Vehic__07C12930");
        });

        modelBuilder.Entity<TourAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK_TourAssignments");
            
            entity.HasOne(d => d.TourSchedule).WithMany(p => p.TourAssignments)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TourAssign_Schedule");

            entity.HasOne(d => d.Account).WithMany(p => p.TourAssignments)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TourAssign_Account");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.TourAssignments)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TourAssign_Vehicle");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK__Vehicles__476B5492A2CB0A2F");

            entity.HasIndex(e => e.PlateNumber, "UQ__Vehicles__03692624F8C844A2").IsUnique();

            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.PlateNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Available");

            entity.HasOne(d => d.Driver).WithMany()
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Vehicle_Driver");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}