using dixanh.Data.Seeds;
using dixanh.Libraries.Models;
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

        // ... ApplyConfigurationsFromAssembly(...) nếu bạn có
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(dixanhDBContext).Assembly);

        modelBuilder.SeedIdentityRoles(); // HasData roles
        modelBuilder.SeedVehicleStatuses(); // HasData vehicle statuses
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