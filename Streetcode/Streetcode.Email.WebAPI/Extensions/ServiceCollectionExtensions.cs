using Hangfire;
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
            var emailConfig = configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
            services.AddSingleton(emailConfig);

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
                config.UseSqlServerStorage(connectionString);
            });

            services.AddHangfireServer();

            var corsConfig = configuration.GetSection("CORS").Get<CorsConfiguration>();
            services.AddCors(opt =>
            {
                opt.AddDefaultPolicy(policy =>
                {
                    if (corsConfig?.AllowedOrigins?.Any() == true && !corsConfig.AllowedOrigins.Contains("*"))
                    {
                        policy.WithOrigins(corsConfig.AllowedOrigins.ToArray());
                    }
                    else
                    {
                        policy.SetIsOriginAllowed(origin => true);
                    }

                    policy.AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            services.AddHsts(opt =>
            {
                opt.Preload = true;
                opt.IncludeSubDomains = true;
                opt.MaxAge = TimeSpan.FromDays(30);
            });

            services.AddLogging();
            services.AddControllers();
        }
        public static void AddCustomServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailConfiguration>(
                configuration.GetSection("EmailConfiguration"));

            services.AddScoped<IEmailService, EmailService>();

        }
    }
}
