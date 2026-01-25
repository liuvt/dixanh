using dixanh.Libraries.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace dixanh.Data;

public partial class dixanhDBContext : IdentityDbContext<AppUser>
{
    //Get config in appsetting
    private readonly IConfiguration configuration;

    public dixanhDBContext(DbContextOptions<dixanhDBContext> options, IConfiguration _configuration) : base(options)
    {
        //Models - Etityties
        configuration = _configuration;
    }

    //Call Model to create table in database

    //Config to connection sql server
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                configuration["ConnectionStrings:Vps"] ??
                    //configuration["ConnectionStrings:Default"] ??
                    throw new InvalidOperationException("Can't find ConnectionStrings in appsettings.json!")
            );
        }

    }

    //Data seeding
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Nếu muốn seed Roles bằng migration thì để đây (nhưng phải tĩnh, không DateTime.Now)
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "1", Name = "Owner", NormalizedName = "OWNER" },
            new IdentityRole { Id = "2", Name = "Administrator", NormalizedName = "ADMINISTRATOR" },
            new IdentityRole { Id = "3", Name = "Manager", NormalizedName = "MANAGER" },
            new IdentityRole { Id = "4", Name = "Driver", NormalizedName = "DRIVER" },
            new IdentityRole { Id = "5", Name = "Employee", NormalizedName = "EMPLOYEE" }
        );

        // BỎ hoàn toàn đoạn HasData cho AspNetUserRoles
        // modelBuilder.Entity<IdentityUserRole<string>>().HasData(...);
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