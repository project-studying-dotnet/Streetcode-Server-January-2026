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
                var environment = services.GetRequiredService<IWebHostEnvironment>();

                await SeedDataAsync(userManager, roleManager, configuration, publishEndpoint, environment);
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

            string testUserId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
            string testUserEmail = "test@example.com";
            string testUserPassword = "Password123_";

            if (await userManager.FindByIdAsync(testUserId) == null)
            {
                var testUser = new ApplicationUser
                {
                    Id = testUserId,
                    Email = testUserEmail,
                    Name = "Test",
                    Surname = "User",
                    UserName = "testuser",
                    EmailConfirmed = true,
                    PhoneNumber = "+380123456789"
                };

                var result = await userManager.CreateAsync(testUser, testUserPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(testUser, nameof(UserRole.User));
                }
            }
        }
    }
}
