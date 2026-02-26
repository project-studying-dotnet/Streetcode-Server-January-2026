using Microsoft.EntityFrameworkCore;
using Streetcode.Email.DAL.Persistence;

namespace Streetcode.Email.WebAPI.Extensions
{
    public static class WebApplicationExtensions
    {
        public static async Task ApplyMigrations(this WebApplication app)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            try
            {
                var emailContext = app.Services.GetRequiredService<EmailDbContext>();
                await emailContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured during startup migration");
            }
        }
    }
}
