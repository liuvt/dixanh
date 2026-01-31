using dixanh.Libraries.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dixanh.Data.FluentAPIs;

public sealed class VehicleCodeHistoryConfiguration : IEntityTypeConfiguration<VehicleCodeHistory>
{
    public void Configure(EntityTypeBuilder<VehicleCodeHistory> b)
    {
        b.ToTable("VehicleCodeHistories");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .HasColumnType("varchar(36)")
            .HasMaxLength(36)
            .IsUnicode(false);

        b.Property(x => x.VehicleId)
            .HasColumnType("varchar(36)")
            .HasMaxLength(36)
            .IsUnicode(false)
            .IsRequired();

        b.Property(x => x.VehicleCode)
            .HasMaxLength(30)
            .IsRequired();

        b.Property(x => x.OperatingArea)
            .HasMaxLength(20)
            .IsRequired();

        b.Property(x => x.ChangeReason)
            .HasMaxLength(255);

        b.Property(x => x.ChangedBy)
            .HasMaxLength(50);

        // Indexes
        b.HasIndex(x => x.VehicleId);
        b.HasIndex(x => new { x.OperatingArea, x.VehicleCode });

        // ✅ Filtered unique index: 1 xe chỉ có 1 code đang hiệu lực
        b.HasIndex(x => x.VehicleId)
            .HasDatabaseName("UX_VehicleCodeHistories_ActivePerVehicle")
            .IsUnique()
            .HasFilter("[ValidTo] IS NULL");

        // ✅ Filtered unique index: trong 1 khu vực, 1 code đang hiệu lực chỉ thuộc 1 xe
        b.HasIndex(x => new { x.OperatingArea, x.VehicleCode })
            .HasDatabaseName("UX_VehicleCodeHistories_ActiveCodePerArea")
            .IsUnique()
            .HasFilter("[ValidTo] IS NULL");

        // Non-unique index phục vụ query lịch sử (tuỳ bạn giữ hoặc bỏ, vì đã có unique index trên VehicleId)
        b.HasIndex(x => x.VehicleId)
            .HasDatabaseName("IX_VehicleCodeHistories_VehicleId");

        b.HasOne(x => x.Vehicle)
            .WithMany(v => v.VehicleCodeHistories)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
