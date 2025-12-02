using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._02_Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AnalysisCallUser._02_Infrastructure.Helpers
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                var context = services.GetRequiredService<AppDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

                try
                {
                    context.Database.Migrate();

                    // 1. Seed Roles
                    await SeedRolesAsync(roleManager, logger);

                    // 2. Seed Default Users
                    await SeedDefaultUsersAsync(userManager, logger);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
        }

        private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
        {
            var rolesToCreate = new[]
            {
            new { Name = "SuperAdmin", Description = "Full system access" },
            new { Name = "Admin", Description = "Can manage users and view reports" },
            new { Name = "User", Description = "Can view reports and search calls" }
        };

            foreach (var roleInfo in rolesToCreate)
            {
                if (!await roleManager.RoleExistsAsync(roleInfo.Name))
                {
                    var role = new ApplicationRole
                    {
                        Name = roleInfo.Name,
                        Description = roleInfo.Description,
                        CreatedAt = DateTime.UtcNow
                    };
                    await roleManager.CreateAsync(role);
                    logger.LogInformation($"Role '{roleInfo.Name}' created.");
                }
            }
        }

        private static async Task SeedDefaultUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
        {
            var defaultUsers = new[]
            {
                new
                {
                    Email = "superadmin@analysis.com",
                    Password = "SuperAdmin@123",
                    FirstName = "Super",
                    LastName = "Admin",
                    Role = "SuperAdmin",
                    ProfilePicture = "default.png" // مقدار پیش‌فرض اضافه شد
                },
                new
                {
                    Email = "admin@analysis.com",
                    Password = "Admin@123",
                    FirstName = "Regular",
                    LastName = "Admin",
                    Role = "Admin",
                    ProfilePicture = "default.png"
                },
                new
                {
                    Email = "user@analysis.com",
                    Password = "User@123",
                    FirstName = "Regular",
                    LastName = "User",
                    Role = "User",
                    ProfilePicture = "default.png"
                }


        };

            foreach (var userInfo in defaultUsers)
            {
                if (await userManager.FindByEmailAsync(userInfo.Email) == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = userInfo.Email,
                        Email = userInfo.Email,
                        FirstName = userInfo.FirstName,
                        LastName = userInfo.LastName,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        ProfilePicture = userInfo.ProfilePicture
                    };

                    var result = await userManager.CreateAsync(user, userInfo.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, userInfo.Role);
                        logger.LogInformation($"Default user '{userInfo.Email}' with role '{userInfo.Role}' created.");
                    }
                    else
                    {
                        logger.LogError($"Error creating user '{userInfo.Email}': {string.Join(", ", result.Errors)}");
                    }
                }
            }
        }
    }
}
