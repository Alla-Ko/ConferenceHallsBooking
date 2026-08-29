using ConferenceHalls.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace ConferenceHalls.Infrastructure.Persistence;

public class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider _services, IConfiguration _configuration)
    {
        var adminEmail = _configuration["Admin:Email"];
        var adminPassword = _configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "Admin credentials are not configured.");
        }

        var roleManager = _services.GetRequiredService<RoleManager<IdentityRole>>();
        await EnsureRoleAsync(roleManager, "Admin");
        await EnsureRoleAsync(roleManager, "User");

        var userManager = _services.GetRequiredService<UserManager<ApplicationUser>>();
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            EnsureSucceeded(
                await userManager.CreateAsync(adminUser, adminPassword),
                "create admin user");
            EnsureSucceeded(
                await userManager.AddToRoleAsync(adminUser, "Admin"),
                "assign Admin role");
        }

        var dbContext = _services.GetRequiredService<AppDbContext>();

        List<HallService> services;
        if (!await dbContext.HallServices.AnyAsync())
        {
            services =
            [
                new HallService { Name = "Проєктор", Price = 500m },
                new HallService { Name = "Wi-Fi", Price = 300m },
                new HallService { Name = "Звук", Price = 700m }
            ];

            dbContext.HallServices.AddRange(services);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            services = await dbContext.HallServices.ToListAsync();
        }

        if (!await dbContext.ConferenceHalls.AnyAsync())
        {
            dbContext.ConferenceHalls.AddRange(
                new ConferenceHall
                {
                    Name = "Зал А",
                    Capacity = 50,
                    HourlyRate = 2000m,
                    IsActive = true,
                    AvailableServices = [.. services]
                },
                new ConferenceHall
                {
                    Name = "Зал B",
                    Capacity = 100,
                    HourlyRate = 3500m,
                    IsActive = true,
                    AvailableServices = [.. services]
                },
                new ConferenceHall
                {
                    Name = "Зал C",
                    Capacity = 30,
                    HourlyRate = 1500m,
                    IsActive = true,
                    AvailableServices = [.. services]
                });

            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task EnsureRoleAsync(
        RoleManager<IdentityRole> roleManager,
        string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        EnsureSucceeded(
            await roleManager.CreateAsync(new IdentityRole(roleName)),
            $"create {roleName} role");
    }

    private static void EnsureSucceeded(IdentityResult result, string action)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to {action}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}