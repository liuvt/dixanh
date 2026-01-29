using dixanh.Libraries.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace dixanh.Data.Seeds;
public static class SeedIdentitys
{
    public static class RoleNames
    {
        public const string Owner = "Owner";
        public const string Administrator = "Administrator";
        public const string Manager = "Manager";
        public const string Driver = "Driver";
        public const string Employee = "Employee";

        public static readonly string[] All = { Owner, Administrator, Manager, Driver, Employee };
    }

    /// <summary>
    /// Seed Roles bằng EF Core HasData (chạy qua migration).
    /// Lưu ý: ConcurrencyStamp nên cố định để tránh EF nghĩ "data changed" mỗi lần migrate.
    /// </summary>
    public static void SeedIdentityRoles(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "1",
                Name = RoleNames.Owner,
                NormalizedName = RoleNames.Owner.ToUpperInvariant(),
                ConcurrencyStamp = "ROLE-OWNER-STATIC"
            },
            new IdentityRole
            {
                Id = "2",
                Name = RoleNames.Administrator,
                NormalizedName = RoleNames.Administrator.ToUpperInvariant(),
                ConcurrencyStamp = "ROLE-ADMIN-STATIC"
            },
            new IdentityRole
            {
                Id = "3",
                Name = RoleNames.Manager,
                NormalizedName = RoleNames.Manager.ToUpperInvariant(),
                ConcurrencyStamp = "ROLE-MANAGER-STATIC"
            },
            new IdentityRole
            {
                Id = "4",
                Name = RoleNames.Driver,
                NormalizedName = RoleNames.Driver.ToUpperInvariant(),
                ConcurrencyStamp = "ROLE-DRIVER-STATIC"
            },
            new IdentityRole
            {
                Id = "5",
                Name = RoleNames.Employee,
                NormalizedName = RoleNames.Employee.ToUpperInvariant(),
                ConcurrencyStamp = "ROLE-EMPLOYEE-STATIC"
            }
        );
    }

    public static async Task SeedIdentityAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        string[] roles = { RoleNames.Owner, RoleNames.Administrator, RoleNames.Manager, RoleNames.Driver, RoleNames.Employee };
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
        await EnsureUser("OWNER-DIXANH-2023", "owner@dixanh.com", "Owner@123", RoleNames.Owner, "Mr.", "Đi xanh");
        await EnsureUser("ADMIN-DIXANH-2023", "administrator@dixanh.com", "Admin@123", RoleNames.Administrator, "QT", "Đi xanh");
        await EnsureUser("MANAGER-DIXANH-2023", "manager@dixanh.com", "Manager@123", RoleNames.Manager, "QL", "Đi xanh");
        await EnsureUser("DRIVER-DIXANH-2023", "driver@dixanh.com", "Driver@123", RoleNames.Driver, "NV", "Tài xế");
        await EnsureUser("EMPLOYEE-DIXANH-2023", "employee@dixanh.com", "Employee@123", RoleNames.Employee, "NV", "Văn Phòng");
    }
}
