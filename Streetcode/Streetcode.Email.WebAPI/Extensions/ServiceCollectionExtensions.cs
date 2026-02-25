using Streetcode.Email.BLL.Configs;
using Streetcode.Email.BLL.Interfaces;
using Streetcode.Email.BLL.Services;

namespace Streetcode.Email.WebAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCustomServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailConfiguration>(
                configuration.GetSection("EmailConfiguration"));

            services.AddScoped<IEmailService, EmailService>();

        }
    }
}
