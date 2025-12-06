using AnalysisCallUser._02_Infrastructure.Data;
using AnalysisCallUser._02_Infrastructure.Helpers;
using AnalysisCallUser._03_EndPoint.Extensions;
using AnalysisCallUser._03_EndPoint.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomServices(builder.Configuration);

builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Clear();
        options.ViewLocationFormats.Add("/03-EndPoint/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/03-EndPoint/Views/Shared/{0}.cshtml");
    });

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "MyApp_";
});

builder.Services.AddSignalR();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCustomMiddlewares();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<LiveDataHub>("/liveDataHub");
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<AnalyticsHub>("/analyticsHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Program>();

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrated successfully.");

        await SeedData.Initialize(services);
        logger.LogInformation("Seed data initialized successfully.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.Run();