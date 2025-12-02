using AnalysisCallUser._02_Infrastructure.Data;
using AnalysisCallUser._02_Infrastructure.Helpers;
using AnalysisCallUser._03_EndPoint.Extensions;
using AnalysisCallUser._03_EndPoint.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCustomServices(builder.Configuration);
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Clear(); // اگر بخواهید فقط مسیر جدید باشد
        options.ViewLocationFormats.Add("/03-EndPoint/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/03-EndPoint/Views/Shared/{0}.cshtml");
    });

// Add MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())//
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCustomMiddlewares(); // افزودن Middlewareها از طریق Extension

app.UseAuthentication();
app.UseAuthorization();

// ثبت و پیکربندی SignalR Hubs
app.MapHub<LiveDataHub>("/liveDataHub");
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<AnalyticsHub>("/analyticsHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// اجرای SeedData برای مقداردهی اولیه دیتابیس
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.Initialize(scope.ServiceProvider);
}


app.Run();