using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
namespace dixanh.Libraries.Models;

public static class Seedding
{
    public static async Task SeedIdentityAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        string[] roles = { "Owner", "Administrator", "Manager", "Driver" };
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
                    PhoneNumber = "0868752251",
                    LockoutEnabled = false
                };

                var create = await userManager.CreateAsync(user, password);
                if (!create.Succeeded)
                    throw new Exception(string.Join("; ", create.Errors.Select(e => e.Description)));
            }

            if (!await userManager.IsInRoleAsync(user, role))
                await userManager.AddToRoleAsync(user, role);
        }

        await EnsureUser("OWNER-dixanh-2023", "owner@dixanh.com", "Owner@123", "Owner", "Đi", "Xanh");
        await EnsureUser("ADMIN-dixanh-2023", "administructor@dixanh.com", "Admin@123", "Administrator", "Đi", "Xanh");
        await EnsureUser("DRIVER-DIXANH-2023", "driver@dixanh.com", "Driver@123", "Driver", "Tài", "Xế");
    }
}
