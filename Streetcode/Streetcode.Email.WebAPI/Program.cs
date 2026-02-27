using Hangfire;
using Streetcode.Email.BLL.Interfaces;
using Streetcode.Email.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddCustomServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.EnvironmentName == "Local")
{
    app.UseHangfireDashboard("/dash");
}
else
{
    app.UseHsts();
}

await app.ApplyMigrations();

// app.SeedDataAsync(); // uncomment for seeding data in local

app.UseHangfireDashboard("/dash");

app.MapControllers();

app.Run();
