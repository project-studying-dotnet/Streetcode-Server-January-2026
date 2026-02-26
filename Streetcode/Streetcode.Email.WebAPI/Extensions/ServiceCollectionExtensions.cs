using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.EntityFrameworkCore;
using Streetcode.Email.BLL.Configs;
using Streetcode.Email.BLL.Interfaces;
using Streetcode.Email.BLL.Services;
using Streetcode.Email.DAL.Persistence;

namespace Streetcode.Email.WebAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services, ConfigurationManager configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var redisConnection = configuration.GetConnectionString("Redis");

            services.AddDbContext<EmailDbContext>(options =>
            {
                options.UseSqlServer(connectionString, opt =>
                {
                    opt.MigrationsAssembly(typeof(EmailDbContext).Assembly.GetName().Name);
                    opt.MigrationsHistoryTable("__EFMigrationsHistory", schema: "entity_framework");
                });
            });

            services.AddHangfire(config =>
            {
                config.UseRedisStorage(redisConnection);
            });

            services.AddHangfireServer();

            services.AddLogging();
            services.AddControllers();
        }
        public static void AddCustomServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailConfiguration>(
                configuration.GetSection("EmailConfiguration"));

            services.AddScoped<IEmailService, EmailService>();

        }

        public static void AddRabbitMQConsumer(this IServiceCollection services)
        {
            services.AddHostedService<EmailConsumer>();
        }

        public class CorsConfiguration
        {
            public List<string> AllowedOrigins { get; set; }
            public List<string> AllowedHeaders { get; set; }
            public List<string> AllowedMethods { get; set; }
            public int PreflightMaxAge { get; set; }
        }

        public class JwtOptions
        {
            public string Key { get; set; }
            public string Issuer { get; set; }
            public string Audience { get; set; }
        }
    }
}
