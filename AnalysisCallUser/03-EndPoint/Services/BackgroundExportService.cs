using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._02_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AnalysisCallUser._03_EndPoint.Services
{
    public class BackgroundExportService : BackgroundService
    {
        private readonly ILogger<BackgroundExportService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public BackgroundExportService(ILogger<BackgroundExportService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Export Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessPendingExports(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // هر یک دقیقه بررسی کن
            }
        }

        private async Task ProcessPendingExports(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var exportService = scope.ServiceProvider.GetRequiredService<IExportService>();
                var realTimeService = scope.ServiceProvider.GetRequiredService<IRealTimeService>();

                var pendingExports = await context.Set<ExportHistory>()
                                             .Where(e => !e.IsCompleted)
                                             .OrderBy(e => e.CreatedAt)
                                             .Take(5) // در هر نوبت حداکثر ۵ را پردازش کن
                                             .ToListAsync(stoppingToken);

                foreach (var export in pendingExports)
                {
                    try
                    {
                        _logger.LogInformation("Processing export {ExportId} for user {UserId}.", export.Id, export.UserId);

                        // تولید فایل
                        var fileBytes = await exportService.GenerateExportFileAsync(export);

                        // ذخیره فایل در مسیر مشخص
                        var filePath = Path.Combine("wwwroot/exports", $"export_{export.Id}.csv");
                        await File.WriteAllBytesAsync(filePath, fileBytes);

                        // به‌روزرسانی رکورد در دیتابیس
                        export.IsCompleted = true;
                        export.IsSuccessful = true;
                        export.FilePath = filePath;
                        export.CompletedAt = DateTime.UtcNow;

                        await context.SaveChangesAsync(stoppingToken);

                        // ارسال نوتیفیکیشن زنده به کاربر
                        await realTimeService.NotifyExportCompleted(export.UserId, filePath);

                        _logger.LogInformation("Successfully processed export {ExportId}.", export.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing export {ExportId}.", export.Id);
                        export.IsCompleted = true;
                        export.IsSuccessful = false;
                        export.ErrorMessage = ex.Message;
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
            }
        }
    }
}
