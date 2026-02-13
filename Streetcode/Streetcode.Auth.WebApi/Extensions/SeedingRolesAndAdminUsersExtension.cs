using Microsoft.AspNetCore.Identity;
using Streetcode.Auth.DAL.Entities;
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

                await SeedDataAsync(userManager, roleManager);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred during database seeding.");
            }
        }

        private static async Task SeedDataAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in Enum.GetNames(typeof(UserRole)))
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = "admin@streetcode.com";
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

                var result = await userManager.CreateAsync(adminUser, "Pa$$w0rdAdmin1!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, nameof(UserRole.Administrator));
                }
            }
        }
    }
}
