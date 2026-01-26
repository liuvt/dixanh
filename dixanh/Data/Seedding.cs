using dixanh.Libraries.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
namespace dixanh.Data;

public static class Seedding
{
    public static async Task SeedIdentityAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        string[] roles = { "Owner", "Administrator", "Manager", "Driver", "Employee" };
        foreach (var r in roles)
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new IdentityRole(r));

        async Task EnsureUser(string id, string email, string password, string role, string first, string last)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new AppUser
                {
                    Id = id,
                    UserName = email,
                    Email = email,
                    FirstName = first,
                    LastName = last,
                    PhoneNumber = "0949492972",
                    LockoutEnabled = false
                };

                var create = await userManager.CreateAsync(user, password);
                if (!create.Succeeded)
                    throw new Exception(string.Join("; ", create.Errors.Select(e => e.Description)));
            }

            if (!await userManager.IsInRoleAsync(user, role))
                await userManager.AddToRoleAsync(user, role);
        }
        await EnsureUser("OWNER-DIXANH-2023", "owner@dixanh.com", "Owner@123", "Owner", "Mr.", "Đi xanh");
        await EnsureUser("ADMIN-DIXANH-2023", "administrator@dixanh.com", "Admin@123", "Administrator", "QT", "Đi xanh");
        await EnsureUser("MANAGER-DIXANH-2023", "manager@dixanh.com", "Manager@123", "Manager", "QL", "Đi xanh");
        await EnsureUser("DRIVER-DIXANH-2023", "driver@dixanh.com", "Driver@123", "Driver", "NV", "Tài xế");
        await EnsureUser("EMPLOYEE-DIXANH-2023", "employee@dixanh.com", "Employee@123", "Employee", "NV", "Văn Phòng");
    }

    public static async Task SeedVehiclesAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<dixanhDBContext>();

        // Đảm bảo DB đã apply migration
        await db.Database.MigrateAsync();

        if (await db.Vehicles.AnyAsync())
            return;

        var vi = CultureInfo.GetCultureInfo("vi-VN");

        DateTime ParseDate(string s)
        {
            var dt = DateTime.ParseExact(s, "dd/MM/yyyy", vi);
            return DateTime.SpecifyKind(dt, DateTimeKind.Local).ToUniversalTime();
        }

        DateTime ParseDateTime(string s)
        {
            var dt = DateTime.ParseExact(s, "dd/MM/yyyy HH:mm:ss", vi);
            return DateTime.SpecifyKind(dt, DateTimeKind.Local).ToUniversalTime();
        }

        var vehicles = new List<Vehicle>
        {
            new Vehicle
            {
                VehicleId = "77daf2429c1aafb52ad7568f8f2558f5",
                LicensePlate = "68G-000.45",
                VehicleCode = "RG7032",
                Brand = "Vinfast VF5",
                SeatCount = 4,
                Color = "Xanh1",
                ManufactureDate = ParseDate("26/01/2026"),
                VehicleType = "Xe điện",
                ChassisNumber = "RLNV5JSE1RH701017",
                EngineNumber = "VFCAFB241080063",
                CreatedBy = "Lưu Văn",
                CreatedAt = ParseDateTime("25/01/2026 16:06:03"),
                UpdatedAt = null,
                StatusId = 1
            },
            new Vehicle
            {
                VehicleId = "77daf2429c1aafb52ad7568f8f2558f1",
                LicensePlate = "68G-000.01",
                VehicleCode = "CM8059",
                Brand = "Vinfast VFe34",
                SeatCount = 4,
                Color = "Xanh",
                ManufactureDate = ParseDate("26/01/2026"),
                VehicleType = "Xe điện",
                ChassisNumber = "RLNV5JSE0PH714757",
                EngineNumber = "VFCAFB23C270090",
                CreatedBy = "Lưu Văn",
                CreatedAt = ParseDateTime("25/01/2026 16:06:03"),
                UpdatedAt = null,
                StatusId = 1
            },
            new Vehicle
            {
                VehicleId = "77daf2429c1aafb52ad7568f8f2558f2",
                LicensePlate = "68F-009.35",
                VehicleCode = "CM8060",
                Brand = "Vinfast Lime",
                SeatCount = 7,
                Color = "Trắng",
                ManufactureDate = ParseDate("26/01/2026"),
                VehicleType = "Xe điện",
                ChassisNumber = "RLNV5JSE9RH700522",
                EngineNumber = "VFCAFB23C270022",
                CreatedBy = "Lưu Văn",
                CreatedAt = ParseDateTime("24/01/2026 00:00:00"),
                UpdatedAt = ParseDateTime("25/01/2026 16:06:03"),
                StatusId = 1
            },
            new Vehicle
            {
                VehicleId = "77daf2429c1aafb52ad7568f8f2558f3",
                LicensePlate = "68E-010.68",
                VehicleCode = "RG7004",
                Brand = "Vinfast VFe34",
                SeatCount = 4,
                Color = "Xanh",
                ManufactureDate = ParseDate("26/01/2026"),
                VehicleType = "Xe điện",
                ChassisNumber = "RLNV5JSE7PH714755",
                EngineNumber = "VFCAFB23C180105",
                CreatedBy = "Lưu Văn",
                CreatedAt = ParseDateTime("25/01/2026 16:06:03"),
                UpdatedAt = null,
                StatusId = 1
            },
            new Vehicle
            {
                VehicleId = "77daf2429c1aafb52ad7568f8f2558f4",
                LicensePlate = "68F-009.10",
                VehicleCode = "AG6123",
                Brand = "Vinfast Lime",
                SeatCount = 7,
                Color = "Vàng",
                ManufactureDate = ParseDate("26/01/2026"),
                VehicleType = "Xe điện",
                ChassisNumber = "RLNV5JSEXPH714765",
                EngineNumber = "VFCAFB23C270074",
                CreatedBy = "Lưu Văn",
                CreatedAt = ParseDateTime("25/01/2026 16:06:03"),
                UpdatedAt = null,
                StatusId = 1
            },
            new Vehicle
            {
                VehicleId = "77daf2429c1aafb52ad7568f8f2558f6",
                LicensePlate = "68G-000.75",
                VehicleCode = "AG6116",
                Brand = "Vinfast VF6",
                SeatCount = 5,
                Color = "Xanh",
                ManufactureDate = ParseDate("26/01/2026"),
                VehicleType = "Xe điện",
                ChassisNumber = "RLNV5JSEXPH714751",
                EngineNumber = "VFCAFB23C270165",
                CreatedBy = "Lưu Văn",
                CreatedAt = ParseDateTime("24/01/2026 00:00:00"),
                UpdatedAt = ParseDateTime("25/01/2026 16:06:03"),
                StatusId = 1
            },
            new Vehicle
            {
                VehicleId = "77daf2429c1aafb52ad7568f8f2558f7",
                LicensePlate = "68E-011.85",
                VehicleCode = "AG6124",
                Brand = "Vinfast VF5",
                SeatCount = 4,
                Color = "Xanh",
                ManufactureDate = ParseDate("26/01/2026"),
                VehicleType = "Xe điện",
                ChassisNumber = "RLNV5JSEXPH714748",
                EngineNumber = "VFCAFB23C190103",
                CreatedBy = "Lưu Văn",
                CreatedAt = ParseDateTime("25/01/2026 16:06:03"),
                UpdatedAt = null,
                StatusId = 1
            }
        };

        db.Vehicles.AddRange(vehicles);
        await db.SaveChangesAsync();
    }
}
