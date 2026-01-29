using dixanh.Libraries.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dixanh.Data.FluentAPIs;

public sealed class VehicleStatusConfiguration : IEntityTypeConfiguration<VehicleStatus>
{
    public void Configure(EntityTypeBuilder<VehicleStatus> b)
    {
        b.HasKey(x => x.StatusId);

        b.HasIndex(x => x.Code).IsUnique();
        b.HasIndex(x => new { x.IsActive, x.SortOrder });

        b.Property(x => x.Code).HasMaxLength(30);
        b.Property(x => x.Name).HasMaxLength(100);
    }
}
