using dixanh.Helpers;
using dixanh.Libraries.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace dixanh.Data.Seeds;

public static class SeedVehicles
{
    public static void SeedVehicleStatuses(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VehicleStatus>().HasData(
            new VehicleStatus { StatusId = 1, Code = "ACTIVE", Name = "Hoạt động", IsActive = true, SortOrder = 1 },
            new VehicleStatus { StatusId = 2, Code = "INACTIVE", Name = "Ngừng hoạt động", IsActive = true, SortOrder = 2 },
            new VehicleStatus { StatusId = 3, Code = "MAINTENANCE", Name = "Bảo trì", IsActive = true, SortOrder = 3 }
        );
    }

    public static async Task SeedVehiclesAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<dixanhDBContext>();

        await db.Database.MigrateAsync();

        if (await db.Vehicles.AnyAsync())
            return;

        var vehicles = new List<Vehicle>
    {
        new Vehicle
        {
            VehicleId = "77daf2429c1aafb52ad7568f8f2558f5",
            LicensePlate = "68G-000.45",
            Brand = "Vinfast VF5",
            SeatCount = 4,
            Color = "Xanh1",
            ManufactureDate = "26/01/2026".ParseVnDateToUtc(),            // DateTimeOffset
            VehicleType = "Xe điện",
            ChassisNumber = "RLNV5JSE1RH701017",
            EngineNumber = "VFCAFB241080063",
            CreatedBy = "Lưu Văn",
            CreatedAt = "17/01/2026 16:06:03".ParseVnDateTimeToUtc(),     // DateTimeOffset
            UpdatedAt = null,
            StatusId = 1
        },
        new Vehicle
        {
            VehicleId = "77daf2429c1aafb52ad7568f8f2558f1",
            LicensePlate = "68G-000.01",
            Brand = "Vinfast VFe34",
            SeatCount = 4,
            Color = "Xanh",
            ManufactureDate = "26/01/2026".ParseVnDateToUtc(),
            VehicleType = "Xe điện",
            ChassisNumber = "RLNV5JSE0PH714757",
            EngineNumber = "VFCAFB23C270090",
            CreatedBy = "Lưu Văn",
            CreatedAt = "05/01/2026 16:06:03".ParseVnDateTimeToUtc(),
            UpdatedAt = null,
            StatusId = 1
        },
        new Vehicle
        {
            VehicleId = "77daf2429c1aafb52ad7568f8f2558f2",
            LicensePlate = "68F-009.35",
            Brand = "Vinfast Lime",
            SeatCount = 7,
            Color = "Trắng",
            ManufactureDate = "26/01/2026".ParseVnDateToUtc(),
            VehicleType = "Xe điện",
            ChassisNumber = "RLNV5JSE9RH700522",
            EngineNumber = "VFCAFB23C270022",
            CreatedBy = "Lưu Văn",
            CreatedAt = "18/01/2026 00:00:00".ParseVnDateTimeToUtc(),
            UpdatedAt = "19/01/2026 11:06:03".ParseVnDateTimeToUtc(),
            StatusId = 2
        },
        new Vehicle
        {
            VehicleId = "77daf2429c1aafb52ad7568f8f2558f3",
            LicensePlate = "68E-010.68",
            Brand = "Vinfast VFe34",
            SeatCount = 4,
            Color = "Xanh",
            ManufactureDate = "01/12/2025".ParseVnDateToUtc(),
            VehicleType = "Xe điện",
            ChassisNumber = "RLNV5JSE7PH714755",
            EngineNumber = "VFCAFB23C180105",
            CreatedBy = "Lưu Văn",
            CreatedAt = "25/01/2026 16:06:03".ParseVnDateTimeToUtc(),
            UpdatedAt = null,
            StatusId = 1
        },
        new Vehicle
        {
            VehicleId = "77daf2429c1aafb52ad7568f8f2558f4",
            LicensePlate = "68F-009.10",
            Brand = "Vinfast Lime",
            SeatCount = 7,
            Color = "Vàng",
            ManufactureDate = "26/01/2026".ParseVnDateToUtc(),
            VehicleType = "Xe điện",
            ChassisNumber = "RLNV5JSEXPH714765",
            EngineNumber = "VFCAFB23C270074",
            CreatedBy = "Lưu Văn",
            CreatedAt = "25/01/2026 16:06:03".ParseVnDateTimeToUtc(),
            UpdatedAt = null,
            StatusId = 3
        },
        new Vehicle
        {
            VehicleId = "77daf2429c1aafb52ad7568f8f2558f6",
            LicensePlate = "68G-000.75",
            Brand = "Vinfast VF6",
            SeatCount = 5,
            Color = "Xanh",
            ManufactureDate = "26/01/2026".ParseVnDateToUtc(),
            VehicleType = "Xe điện",
            ChassisNumber = "RLNV5JSEXPH714751",
            EngineNumber = "VFCAFB23C270165",
            CreatedBy = "Lưu Văn",
            CreatedAt = "01/12/2025 00:00:00".ParseVnDateTimeToUtc(),
            UpdatedAt = "10/01/2026 06:06:03".ParseVnDateTimeToUtc(),
            StatusId = 2
        },
        new Vehicle
        {
            VehicleId = "77daf2429c1aafb52ad7568f8f2558f7",
            LicensePlate = "68E-011.85",
            Brand = "Vinfast VF5",
            SeatCount = 4,
            Color = "Xanh",
            ManufactureDate = "26/01/2026".ParseVnDateToUtc(),
            VehicleType = "Xe điện",
            ChassisNumber = "RLNV5JSEXPH714748",
            EngineNumber = "VFCAFB23C190103",
            CreatedBy = "Lưu Văn",
            CreatedAt = "20/01/2026 16:06:03".ParseVnDateTimeToUtc(),
            UpdatedAt = null,
            StatusId = 1
        }
    };

        db.Vehicles.AddRange(vehicles);
        await db.SaveChangesAsync();
    }
}
