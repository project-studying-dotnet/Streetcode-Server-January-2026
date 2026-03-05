using FluentValidation;
using Hangfire;
using Hangfire.SqlServer;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.Email.BLL.Configs;
using Streetcode.Email.BLL.Interfaces;
using Streetcode.Email.BLL.MediatR.Behavior;
using Streetcode.Email.BLL.Services;
using Streetcode.Email.DAL.Persistence;
using System.Reflection;

namespace Streetcode.Email.WebAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services, ConfigurationManager configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<EmailDbContext>(options =>
            {
                options.UseSqlServer(connectionString, opt =>
                {
                    opt.MigrationsAssembly(typeof(EmailDbContext).Assembly.GetName().Name);
                    opt.MigrationsHistoryTable("__EFMigrationsHistory", schema: "entity_framework");
                });
            });

            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    PrepareSchemaIfNecessary = true,

                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

            services.AddHangfireServer();

            services.AddLogging();

            services.AddControllers();
        }
        public static void AddCustomServices(this IServiceCollection services, IConfiguration configuration)
        {
            var bllAssembly = Assembly.Load("Streetcode.Email.BLL");

            services.AddAutoMapper(bllAssembly);
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(bllAssembly));
            services.Configure<EmailConfiguration>(
                configuration.GetSection("EmailConfiguration"));

            services.AddScoped<IEmailService, EmailService>();

            services.AddValidatorsFromAssembly(bllAssembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorBehavior<,>));

            services.AddMassTransit(x =>
            {
                x.AddConsumer<EmailConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitSection = configuration.GetSection("RabbitMQ");

                    var host = rabbitSection["Host"]
                        ?? throw new InvalidOperationException("RabbitMQ Host is missing");
                    var username = rabbitSection["Username"]
                        ?? throw new InvalidOperationException("RabbitMQ Username is missing");
                    var password = rabbitSection["Password"]
                        ?? throw new InvalidOperationException("RabbitMQ Password is missing");

                    cfg.Host(host, "/", h =>
                    {
                        h.Username(username);
                        h.Password(password);
                    });
                    cfg.ConfigureEndpoints(context);
                });
            });
        }
    }
}