using dixanh.Libraries.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace dixanh.Data;

public partial class dixanhDBContext : IdentityDbContext<AppUser>
{
    public dixanhDBContext(DbContextOptions<dixanhDBContext> options) : base(options) { }

    //Call Model to create table in database
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleStatus> VehicleStatuses => Set<VehicleStatus>();
    public DbSet<VehicleStatusHistory> VehicleStatusHistories => Set<VehicleStatusHistory>();

    public DbSet<CooperationProfile> CooperationProfiles => Set<CooperationProfile>();

    //Data seeding
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        #region Fluent API: Vehicle - VehicleStatus - VehicleStatusHistory 
        // -----------------------------
        // VEHICLE
        // -----------------------------
        modelBuilder.Entity<Vehicle>()
            .HasKey(v => v.VehicleId);

        modelBuilder.Entity<Vehicle>()
            .Property(x => x.VehicleId)
            .HasMaxLength(36)
            .IsUnicode(false);

        modelBuilder.Entity<Vehicle>()
            .HasIndex(x => x.LicensePlate).IsUnique();
        modelBuilder.Entity<Vehicle>()
            .HasIndex(x => new { x.StatusId, x.CreatedAt });
        modelBuilder.Entity<Vehicle>()
            .HasIndex(x => x.VehicleCode);

        // Vehicle -> VehicleStatus (không cascade)
        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Status)
            .WithMany(s => s.Vehicles)
            .HasForeignKey(v => v.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // -----------------------------
        // VEHICLE STATUS
        // -----------------------------
        modelBuilder.Entity<VehicleStatus>()
            .HasIndex(x => x.Code)
            .IsUnique();

        modelBuilder.Entity<VehicleStatus>()
            .HasIndex(x => new { x.IsActive, x.SortOrder });

        // -----------------------------
        // VEHICLE STATUS HISTORY
        // -----------------------------

        // History -> Vehicle (map đúng FK VehicleId, không tạo shadow)
        modelBuilder.Entity<VehicleStatusHistory>()
            .Property(x => x.VehicleId)
            .HasMaxLength(36)
            .IsUnicode(false);

        modelBuilder.Entity<VehicleStatusHistory>()
            .HasOne(h => h.Vehicle)
            .WithMany(v => v.StatusHistories)
            .HasForeignKey(h => h.VehicleId)
            .HasPrincipalKey(v => v.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        // History -> VehicleStatus (From/To) - không cascade
        modelBuilder.Entity<VehicleStatusHistory>()
            .HasOne(h => h.ToStatus)
            .WithMany()
            .HasForeignKey(h => h.ToStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VehicleStatusHistory>()
            .HasOne(h => h.FromStatus)
            .WithMany()
            .HasForeignKey(h => h.FromStatusId)
            .OnDelete(DeleteBehavior.Restrict);
        #endregion

        #region Data Seeding: Identity Roles & VehicleStatus
        // Roles
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "1", Name = "Owner", NormalizedName = "OWNER" },
            new IdentityRole { Id = "2", Name = "Administrator", NormalizedName = "ADMINISTRATOR" },
            new IdentityRole { Id = "3", Name = "Manager", NormalizedName = "MANAGER" },
            new IdentityRole { Id = "4", Name = "Driver", NormalizedName = "DRIVER" },
            new IdentityRole { Id = "5", Name = "Employee", NormalizedName = "EMPLOYEE" }
        );

        // VehicleStatus
        modelBuilder.Entity<VehicleStatus>().HasData(
            new VehicleStatus { StatusId = 1, Code = "ACTIVE", Name = "Hoạt động", IsActive = true, SortOrder = 1 },
            new VehicleStatus { StatusId = 2, Code = "INACTIVE", Name = "Ngừng hoạt động", IsActive = true, SortOrder = 2 },
            new VehicleStatus { StatusId = 3, Code = "MAINTENANCE", Name = "Bảo trì", IsActive = true, SortOrder = 3 }
        );
        #endregion
    }
}

//Update tool: dotnet tool update --global dotnet-ef

//Create mirations: dotnet ef migrations add Init -o Data/Migrations
//Create database: dotnet ef database update

/* 
 * ///Publish project: 
dotnet publish -c Release --output ./Publish dixanh.csproj

 * ///Tailwind project: 
npx @tailwindcss/cli -i ./dixanh/TailwindImport/input.css -o ./dixanh/wwwroot/css/tailwindcss.css --watch 
*/