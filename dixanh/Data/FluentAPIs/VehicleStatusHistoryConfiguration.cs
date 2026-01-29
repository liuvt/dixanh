using dixanh.Libraries.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dixanh.Data.FluentAPIs;

public sealed class VehicleStatusHistoryConfiguration : IEntityTypeConfiguration<VehicleStatusHistory>
{
    public void Configure(EntityTypeBuilder<VehicleStatusHistory> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.VehicleId)
            .HasMaxLength(36)
            .IsUnicode(false);

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
