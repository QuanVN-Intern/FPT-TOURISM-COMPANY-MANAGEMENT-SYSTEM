using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

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

    public virtual DbSet<TourVehicle> TourVehicles { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-E9VL67H;Database=TravelCompanyDB;User Id=sa;Password=123456;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TourVehicle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourVehi__3214EC074B5E385F");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.TourVehicles)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TourVehic__Vehic__07C12930");
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
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
