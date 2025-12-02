using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._02_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AnalysisCallUser._03_EndPoint.BackgroundServices
{
    public class DataCleanupService : BackgroundService
    {
        private readonly ILogger<DataCleanupService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DataCleanupService(ILogger<DataCleanupService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Data Cleanup Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Running data cleanup task at: {time}", DateTimeOffset.Now);

                await CleanupOldLoginHistories(stoppingToken);
                await CleanupOldExportFiles(stoppingToken);

                // هر ۲۴ ساعت یکبار اجرا شو
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task CleanupOldLoginHistories(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var cutoffDate = DateTime.UtcNow.AddDays(-90); // حذف لاگ‌های قدیمی‌تر از ۹۰ روز

                var oldHistories = await context.Set<UserLoginHistory>()
                                             .Where(h => h.LoginTime < cutoffDate)
                                             .ToListAsync(stoppingToken);

                if (oldHistories.Any())
                {
                    context.Set<UserLoginHistory>().RemoveRange(oldHistories);
                    await context.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Deleted {count} old login history entries.", oldHistories.Count);
                }
            }
        }

        private async Task CleanupOldExportFiles(CancellationToken stoppingToken)
        {
            var exportDirectory = Path.Combine("wwwroot", "exports");
            if (!Directory.Exists(exportDirectory)) return;

            var files = Directory.GetFiles(exportDirectory, "*.csv");
            var cutoffDate = DateTime.UtcNow.AddDays(-7); // حذف فایل‌های قدیمی‌تر از ۷ روز

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    try
                    {
                        File.Delete(file);
                        _logger.LogInformation("Deleted old export file: {fileName}", file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Could not delete file: {fileName}", file);
                    }
                }
            }
        }
    }
}
