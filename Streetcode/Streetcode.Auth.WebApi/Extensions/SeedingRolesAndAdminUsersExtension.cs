using MassTransit;
using Microsoft.AspNetCore.Identity;
using Streetcode.Auth.DAL.Entities;
using Streetcode.Shared.DTO.Events;
using Streetcode.Shared.Enums;

namespace Streetcode.Auth.WebApi.Extensions
{
    public static class SeedingRolesAndAdminUsersExtension
    {
        public static async Task ApplySeedingAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var configuration = services.GetRequiredService<IConfiguration>();
                var publishEndpoint = services.GetRequiredService<IPublishEndpoint>();

                await SeedDataAsync(userManager, roleManager, configuration, publishEndpoint);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred during database seeding.");
            }
        }

        private static async Task SeedDataAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IPublishEndpoint publishEndpoint)
        {
            foreach (var roleName in Enum.GetNames(typeof(UserRole)))
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = configuration["Admin:Email"]
                ?? throw new InvalidOperationException("Admin Email is missing in configuration");
            var adminPassword = configuration["Admin:Password"]
                ?? throw new InvalidOperationException("Admin Password is missing in configuration");

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = "Admin",
                    Surname = "SuperUser",
                    EmailConfirmed = true,
                    PhoneNumber = "+380670000000"
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, nameof(UserRole.Administrator));

                    await publishEndpoint.Publish(new UserRegisteredEvent
                    {
                        UserId = adminUser.Id,
                        Email = adminUser.Email,
                        Name = adminUser.Name,
                        Surname = adminUser.Surname,
                        PhoneNumber = adminUser.PhoneNumber,
                        Role = UserRole.Administrator
                    });
                }
            }
        }
    }
}
