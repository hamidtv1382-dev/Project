using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._01_Domain.Services;
using AnalysisCallUser._02_Infrastructure.Data;
using AnalysisCallUser._02_Infrastructure.Identity;
using AnalysisCallUser._02_Infrastructure.Repository.Base;
using AnalysisCallUser._02_Infrastructure.Repository.Repositories;
using AnalysisCallUser._03_EndPoint.BackgroundServices;
using AnalysisCallUser._03_EndPoint.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace AnalysisCallUser._03_EndPoint.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
        {
            // --------------------------
            // DbContext
            // --------------------------
            services.AddDbContext<AppDbContext>(options =>
       options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
             );

            // --------------------------
            // Identity
            // --------------------------
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // --------------------------
            // Custom Identity Services
            // --------------------------
            services.AddScoped<CustomUserManager>();
            services.AddScoped<CustomSignInManager>();
            services.AddScoped<RoleManagerService>();

            // --------------------------
            // UnitOfWork and Repositories
            // --------------------------
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICallDetailRepository, CallDetailRepository>();
            services.AddScoped<ICallTypeRepository, CallTypeRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<IOperatorRepository, OperatorRepository>();

            // --------------------------
            // Domain Services
            // --------------------------
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICallAnalysisService, CallAnalysisService>();
            services.AddScoped<IExportService, ExportService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IFilterService, FilterService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();

            // --------------------------
            // RealTime Service (EndPoint)
            // --------------------------
            services.AddScoped<IRealTimeService, AnalysisCallUser._03_EndPoint.Services.RealTimeService>();

            // --------------------------
            // Endpoint / Application Services
            // --------------------------
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IThemeService, ThemeService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<BackgroundExportService>();

            // --------------------------
            // SignalR Hubs
            // --------------------------
            services.AddSignalR();

            // --------------------------
            // In-Memory Cache
            // --------------------------
            services.AddMemoryCache();

            // --------------------------
            // FluentValidation (نسخه جدید)
            // --------------------------
            services.Scan(scan => scan
                .FromAssemblies(Assembly.GetExecutingAssembly())
                .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            );
            // --------------------------
            // Background Services (IHostedService)
            // --------------------------
            services.AddHostedService<DataCleanupService>();
            services.AddHostedService<CacheWarmupService>();
            services.AddHostedService<ExportProcessingService>();

            return services;
        }
    }
}
