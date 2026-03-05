using Microsoft.EntityFrameworkCore;
using Streetcode.Email.DAL.Persistence;

namespace Streetcode.Email.WebAPI.Extensions
{
    public static class WebApplicationExtensions
    {
        public static async Task ApplyMigrations(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                var emailContext = services.GetRequiredService<EmailDbContext>();
                await emailContext.Database.MigrateAsync();
                logger.LogInformation("Database migrated successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during startup migration");
                throw;
            }
        }
    }
}
