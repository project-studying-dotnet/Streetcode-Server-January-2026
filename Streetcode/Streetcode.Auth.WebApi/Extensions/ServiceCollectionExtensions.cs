using FluentValidation;
using Hangfire;
using Hangfire.SqlServer;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Streetcode.Auth.BLL.DTO.Options;
using Streetcode.Auth.BLL.Interfaces;
using Streetcode.Auth.BLL.MediatR.Behaviors;
using Streetcode.Auth.BLL.Services;
using Streetcode.Auth.DAL.Entities;
using Streetcode.Auth.DAL.Persistence;
using Streetcode.Auth.DAL.Repositories.Interfaces;
using Streetcode.Auth.DAL.Repositories.Realizations;
using Streetcode.Auth.WebApi.Services.Interfaces;
using Streetcode.Auth.WebApi.Services.Realizations;
using System.Reflection;
using System.Text;

namespace Streetcode.Auth.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddDataAccessServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseSqlServer(connectionString, opt =>
            {
                opt.MigrationsAssembly(typeof(AuthDbContext).Assembly.GetName().Name);
                opt.MigrationsHistoryTable("__EFMigrationsHistory", schema: "entity_framework");
            });
        });

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    }

    public static void AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<AuthDbContext>()
        .AddDefaultTokenProviders();

        var jwtOptions = new JwtOptionsDTO();
        configuration.GetSection("Jwt").Bind(jwtOptions);

        services.Configure<JwtOptionsDTO>(configuration.GetSection("Jwt"));

        var key = Encoding.UTF8.GetBytes(jwtOptions.Key);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        })
        .AddGoogle(options =>
        {
            options.ClientId = configuration["Authentication:Google:ClientId"];
            options.ClientSecret = configuration["Authentication:Google:ClientSecret"];

            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });
    }

    public static void AddCorsServices(this IServiceCollection services, IConfiguration configuration)
    {
        var corsConfig = configuration.GetSection("CORS").Get<CorsConfigurationDTO>()
                         ?? throw new InvalidOperationException("CORS configuration is missing");

        services.AddCors(opt =>
        {
            opt.AddPolicy("CorsPolicy", policy =>
            {
                policy.WithOrigins(corsConfig.AllowedOrigins.ToArray())
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        services.AddHsts(opt =>
        {
            opt.Preload = true;
            opt.IncludeSubDomains = true;
            opt.MaxAge = TimeSpan.FromDays(10);
        });
    }

    public static void AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        var bllAssembly = Assembly.Load("Streetcode.Auth.BLL");

        services.AddAutoMapper(bllAssembly);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(bllAssembly));

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenCookieService, RefreshTokenCookieService>();

        services.AddValidatorsFromAssembly(bllAssembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorBehavior<,>));

        services.AddMassTransit(x =>
        {
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

    public static void AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(opt =>
        {
            opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Streetcode Auth API", Version = "v1" });

            opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            opt.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
        });
    }

    public static void AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                PrepareSchemaIfNecessary = true
            });
        });

        services.AddHangfireServer();
    }
}