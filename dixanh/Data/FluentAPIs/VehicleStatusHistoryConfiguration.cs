using dixanh.Libraries.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dixanh.Data.FluentAPIs;

public sealed class VehicleStatusHistoryConfiguration : IEntityTypeConfiguration<VehicleStatusHistory>
{
    public void Configure(EntityTypeBuilder<VehicleStatusHistory> b)
    {
        b.ToTable("VehicleStatusHistories");

        b.HasKey(x => x.Id);

        b.Property(x => x.VehicleId)
            .HasColumnType("varchar(36)")
            .HasMaxLength(36)
            .IsUnicode(false)
            .IsRequired();

        b.Property(x => x.ChangedBy)
            .HasMaxLength(50);

        b.Property(x => x.Note)
            .HasMaxLength(255);

        b.Property(x => x.ToStatusId).IsRequired();

        b.HasIndex(x => new { x.VehicleId, x.ChangedAt });

        // History -> Vehicle
        b.HasOne(x => x.Vehicle)
            .WithMany(v => v.StatusHistories)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        // History -> ToStatus / FromStatus (no cascade)
        b.HasOne(x => x.ToStatus)
            .WithMany()
            .HasForeignKey(x => x.ToStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.FromStatus)
            .WithMany()
            .HasForeignKey(x => x.FromStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
