using FluentValidation;
using Hangfire;
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

            services.AddHangfire(config =>
            {
                config.UseSqlServerStorage(connectionString);
            });

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

                    cfg.Host(rabbitSection["Host"], "/", h =>
                    {
                        h.Username(rabbitSection["Username"]);
                        h.Password(rabbitSection["Password"]);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

        }
    }
}
