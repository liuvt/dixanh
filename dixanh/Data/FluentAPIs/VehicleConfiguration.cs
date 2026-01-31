using dixanh.Libraries.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace dixanh.Data.FluentAPIs;

public sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> b)
    {

        b.ToTable("Vehicles");

        // PK
        b.HasKey(x => x.VehicleId);

        b.Property(x => x.VehicleId)
            .HasColumnType("varchar(36)")
            .HasMaxLength(36)
            .IsUnicode(false);

        // CurrentVehicleCode
        b.Property(x => x.CurrentVehicleCode)
            .HasMaxLength(30);

        // Core fields
        b.Property(x => x.LicensePlate)
            .HasMaxLength(20)
            .IsRequired();

        b.Property(x => x.Brand)
            .HasMaxLength(50);

        b.Property(x => x.Color)
            .HasMaxLength(30);

        b.Property(x => x.VehicleType)
            .HasMaxLength(30);

        b.Property(x => x.ChassisNumber)
            .HasMaxLength(50);

        b.Property(x => x.EngineNumber)
            .HasMaxLength(50);

        b.Property(x => x.CreatedBy)
            .HasMaxLength(50);

        // Time
        b.Property(x => x.CreatedAt);
        b.Property(x => x.UpdatedAt);
        b.Property(x => x.ManufactureDate);

        // Indexes
        b.HasIndex(x => x.LicensePlate).IsUnique();
        b.HasIndex(x => x.CurrentVehicleCode);
        b.HasIndex(x => new { x.StatusId, x.CreatedAt });
        b.HasIndex(x => x.CreatedAt);
        b.HasIndex(x => x.StatusId);

        // Vehicle -> VehicleStatus (no cascade)
        b.HasOne(x => x.Status)
            .WithMany(s => s.Vehicles)
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Vehicle -> StatusHistories
        b.HasMany(x => x.StatusHistories)
            .WithOne(h => h.Vehicle)
            .HasForeignKey(h => h.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Vehicle -> VehicleCodeHistories
        b.HasMany(x => x.VehicleCodeHistories)
            .WithOne(h => h.Vehicle)
            .HasForeignKey(h => h.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        // CooperationProfiles bạn đang dùng rồi: nếu muốn chốt FK theo VehicleId thì config ở CooperationProfileConfiguration

    }
}
