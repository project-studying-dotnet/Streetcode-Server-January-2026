using Microsoft.EntityFrameworkCore;
using Streetcode.Auth.DAL.Persistence;

namespace Streetcode.Auth.WebApi.Extensions
{
    public static class MigrationExtensions
    {
        public static async Task ApplyMigrationsAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    var context = services.GetRequiredService<AuthDbContext>();

                    if ((await context.Database.GetPendingMigrationsAsync()).Any())
                    {
                        logger.LogInformation("Applying pending migrations.");
                        await context.Database.MigrateAsync();
                        logger.LogInformation("Migrations applied successfully.");
                    }
                    else
                    {
                        logger.LogInformation("No pending migrations found.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database.");
                    throw;
                }
            }
        }
    }
}
