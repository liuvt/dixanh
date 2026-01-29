using dixanh.Libraries.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dixanh.Data.FluentAPIs;

public sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> b)
    {
        b.HasKey(x => x.VehicleId);

        b.Property(x => x.VehicleId)
            .HasMaxLength(36)
            .IsUnicode(false);

        b.HasIndex(x => x.LicensePlate).IsUnique();
        b.HasIndex(x => new { x.StatusId, x.CreatedAt });
        b.HasIndex(x => x.VehicleCode);

        // Vehicle -> VehicleStatus (no cascade)
        b.HasOne(x => x.Status)
            .WithMany(s => s.Vehicles)
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Vehicle -> History
        b.HasMany(x => x.StatusHistories)
            .WithOne(h => h.Vehicle)
            .HasForeignKey(h => h.VehicleId)
            .HasPrincipalKey(v => v.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
