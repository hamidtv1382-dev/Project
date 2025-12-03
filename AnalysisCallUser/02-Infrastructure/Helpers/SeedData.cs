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

                    // 1. Seed Countries
                    await SeedCountriesAsync(context, logger);

                    // 2. Seed Cities
                    await SeedCitiesAsync(context, logger);

                    // 3. Seed Operators
                    await SeedOperatorsAsync(context, logger);

                    // 4. Seed Call Types
                    await SeedCallTypesAsync(context, logger);

                    // 5. Seed Roles
                    await SeedRolesAsync(roleManager, logger);

                    // 6. Seed Default Users
                    await SeedDefaultUsersAsync(userManager, logger);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
        }

        private static async Task SeedCountriesAsync(AppDbContext context, ILogger logger)
        {
            if (!await context.Countries.AnyAsync())
            {
                var countries = new[]
                {
                    new Country { CountryName = "Iran" },
                    new Country { CountryName = "Azerbaijan" }
                };

                await context.Countries.AddRangeAsync(countries);
                await context.SaveChangesAsync();
                logger.LogInformation("Countries seeded successfully.");
            }
        }

        private static async Task SeedCitiesAsync(AppDbContext context, ILogger logger)
        {
            if (!await context.Cities.AnyAsync())
            {
                // Get country IDs
                var iran = await context.Countries.FirstOrDefaultAsync(c => c.CountryName == "Iran");
                var azerbaijan = await context.Countries.FirstOrDefaultAsync(c => c.CountryName == "Azerbaijan");

                var cities = new List<City>();

                // Iran cities
                if (iran != null)
                {
                    cities.AddRange(new[]
                    {
                        new City { CityName = "Alborz", CountryID = iran.CountryID },
                        new City { CityName = "Ardabil province", CountryID = iran.CountryID },
                        new City { CityName = "East Azarbaijan", CountryID = iran.CountryID },
                        new City { CityName = "Gilan", CountryID = iran.CountryID },
                        new City { CityName = "Golestan", CountryID = iran.CountryID },
                        new City { CityName = "Isfahan province", CountryID = iran.CountryID },
                        new City { CityName = "Kermanshah province", CountryID = iran.CountryID },
                        new City { CityName = "Khuzestan", CountryID = iran.CountryID },
                        new City { CityName = "Kurdistan", CountryID = iran.CountryID },
                        new City { CityName = "Mazandaran", CountryID = iran.CountryID },
                        new City { CityName = "Qazvin province", CountryID = iran.CountryID },
                        new City { CityName = "Qom province", CountryID = iran.CountryID },
                        new City { CityName = "Razavi Khorasan", CountryID = iran.CountryID },
                        new City { CityName = "Tehran province", CountryID = iran.CountryID },
                        new City { CityName = "West Azarbaijan", CountryID = iran.CountryID },
                        new City { CityName = "Zanjan province", CountryID = iran.CountryID }
                    });
                }

                // Azerbaijan cities
                if (azerbaijan != null)
                {
                    cities.AddRange(new[]
                    {
                        new City { CityName = "Astara", CountryID = azerbaijan.CountryID },
                        new City { CityName = "Babek", CountryID = azerbaijan.CountryID },
                        new City { CityName = "Baku", CountryID = azerbaijan.CountryID },
                        new City { CityName = "Ganja", CountryID = azerbaijan.CountryID },
                        new City { CityName = "Guba", CountryID = azerbaijan.CountryID },
                        new City { CityName = "Jalilabad", CountryID = azerbaijan.CountryID },
                        new City { CityName = "Julfa", CountryID = azerbaijan.CountryID },
                        new City { CityName = "Kangarli", CountryID = azerbaijan.CountryID },
                        new City { CityName = "Lankaran", CountryID = azerbaijan.CountryID },
                        new City { CityName = "Lerik", CountryID = azerbaijan.CountryID },
                        new City { CityName = "Nakhchivan city", CountryID = azerbaijan.CountryID },
                        new City { CityName = "Salyan", CountryID = azerbaijan.CountryID },
                        new City { CityName = "Shaki", CountryID = azerbaijan.CountryID },
                        new City { CityName = "Shamkir", CountryID = azerbaijan.CountryID },
                        new City { CityName = "Sharur", CountryID = azerbaijan.CountryID },
                        new City { CityName = "Tartar", CountryID = azerbaijan.CountryID }
                    });
                }

                await context.Cities.AddRangeAsync(cities);
                await context.SaveChangesAsync();
                logger.LogInformation("Cities seeded successfully.");
            }
        }

        private static async Task SeedOperatorsAsync(AppDbContext context, ILogger logger)
        {
            if (!await context.Operators.AnyAsync())
            {
                var operators = new[]
                {
                    new Operator { OperatorName = "Azercell" },
                    new Operator { OperatorName = "Bakcell" },
                    new Operator { OperatorName = "FONEX" },
                    new Operator { OperatorName = "IR-MCI" },
                    new Operator { OperatorName = "Irancell" },
                    new Operator { OperatorName = "Nar Mobile" },
                    new Operator { OperatorName = "Naxtel" },
                    new Operator { OperatorName = "Rightel" },
                    new Operator { OperatorName = "Shatel Mobile" }
                };

                await context.Operators.AddRangeAsync(operators);
                await context.SaveChangesAsync();
                logger.LogInformation("Operators seeded successfully.");
            }
        }

        private static async Task SeedCallTypesAsync(AppDbContext context, ILogger logger)
        {
            if (!await context.CallTypes.AnyAsync())
            {
                var callTypes = new[]
                {
                    new CallType { TypeName = "Local" },
                    new CallType { TypeName = "National" },
                    new CallType { TypeName = "International" },
                    new CallType { TypeName = "Roaming" },
                    new CallType { TypeName = "Emergency" }
                };

                await context.CallTypes.AddRangeAsync(callTypes);
                await context.SaveChangesAsync();
                logger.LogInformation("Call types seeded successfully.");
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
                    ProfilePicture = "default.png"
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