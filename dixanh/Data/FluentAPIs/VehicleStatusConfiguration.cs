using dixanh.Libraries.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dixanh.Data.FluentAPIs;

public sealed class VehicleStatusConfiguration : IEntityTypeConfiguration<VehicleStatus>
{
    public void Configure(EntityTypeBuilder<VehicleStatus> b)
    {
        b.ToTable("VehicleStatuses");

        b.HasKey(x => x.StatusId);

        b.Property(x => x.Code)
            .HasMaxLength(30)
            .IsRequired();

        b.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        b.HasIndex(x => x.Code).IsUnique();
        b.HasIndex(x => new { x.IsActive, x.SortOrder });

    }
}
