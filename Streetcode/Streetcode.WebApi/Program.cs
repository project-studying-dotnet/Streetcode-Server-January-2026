using Hangfire;
using Streetcode.BLL.Services.BlobStorageService;
using Streetcode.WebApi.Extensions;
using Streetcode.WebApi.Middleware;
using Streetcode.WebApi.Utils;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureApplication();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerServices();
builder.Services.AddRedisCacheServices(builder.Configuration);
builder.Services.AddCustomServices(builder.Configuration, builder.Environment);
builder.Services.ConfigureBlob(builder);
builder.Services.ConfigurePayment(builder);
builder.Services.ConfigureInstagram(builder);
builder.Services.ConfigureSerilog(builder);
var app = builder.Build();

if (app.Environment.EnvironmentName == "Local")
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIv5 v1"));
}
else
{
    app.UseHsts();
}

await app.ApplyMigrations();

// app.SeedDataAsync(); // uncomment for seeding data in local
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseCors();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/dash");

if (app.Environment.EnvironmentName != "Local")
{
    var bypassSslValidation = app.Configuration.GetValue<bool>("BypassSslValidation");
    BackgroundJob.Schedule<WebParsingUtils>(
    wp => wp.ParseZipFileFromWebAsync(bypassSslValidation), TimeSpan.FromMinutes(1));
    RecurringJob.AddOrUpdate<WebParsingUtils>(
        wp => wp.ParseZipFileFromWebAsync(bypassSslValidation), Cron.Monthly);
    RecurringJob.AddOrUpdate<AzureBlobService>(
        b => b.CleanBlobStorage(), Cron.Monthly);
}

app.MapControllers();

app.Run();
public partial class Program
{
}