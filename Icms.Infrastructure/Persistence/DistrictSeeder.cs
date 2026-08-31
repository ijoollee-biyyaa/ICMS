using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Icms.Domain;
using Icms.Domain.Enums;
using Icms.Infrastructure.Identity;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence;

public static class DistrictSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IcmsDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var districtId = configuration.GetValue<long>("IcmsDefaults:DistrictId", 1);

        // 1. Ensure district exists
        var district = await context.Districts
            .AsNoTracking()
            .SingleOrDefaultAsync(d => d.Id == districtId && !d.IsDeleted);

        if (district is null)
        {
            district = new District
            {
                Id = districtId,
                Name = "North West Addis Ababa District Office",
                Code = "NWAA-DO",
                Address = "District Headquarters",
                CreatedAt = DateTimeOffset.UtcNow,
            };
            context.Districts.Add(district);
            await context.SaveChangesAsync();
        }

        // 2. Create roles if missing
        await EnsureRoleAsync(roleManager, "Admin");
        await EnsureRoleAsync(roleManager, "DistrictSubAdmin");
        await EnsureRoleAsync(roleManager, "ChurchAdmin");
        await EnsureRoleAsync(roleManager, "ChurchSubAdmin");
        await EnsureRoleAsync(roleManager, "Member");

// 3. Seeder does NOT create the president employee.
// The president employee is created via the DistrictEmployeeService.CreateEmployeeAsync
// endpoint when the first district staff is hired with IsDistrictPresident=true.
// That endpoint validates only one president exists and sets the flags correctly.
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}