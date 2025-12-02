using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._02_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AnalysisCallUser._03_EndPoint.BackgroundServices
{
    public class ExportProcessingService : BackgroundService
    {
        private readonly ILogger<ExportProcessingService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ExportProcessingService(ILogger<ExportProcessingService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Export Processing Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessPendingExports(stoppingToken);

                // هر ۳۰ ثانیه یکبار صف را چک کن
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
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
                                             .Take(3) // همزمان حداکثر ۳ اکسپورت را پردازش کن
                                             .ToListAsync(stoppingToken);

                foreach (var export in pendingExports)
                {
                    _logger.LogInformation("Processing export {ExportId} for user {UserId}.", export.Id, export.UserId);

                    try
                    {
                        // تولید فایل اکسپورت
                        var fileBytes = await exportService.GenerateExportFileAsync(export);

                        // ذخیره فایل در سرور
                        var fileName = $"export_{export.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
                        var filePath = Path.Combine("wwwroot", "exports", fileName);
                        await File.WriteAllBytesAsync(filePath, fileBytes);

                        // به‌روزرسانی رکورد در دیتابیس
                        export.IsCompleted = true;
                        export.IsSuccessful = true;
                        export.FilePath = $"/exports/{fileName}";
                        export.CompletedAt = DateTime.UtcNow;

                        await context.SaveChangesAsync(stoppingToken);

                        // ارسال اعلان به کاربر از طریق SignalR
                        await realTimeService.NotifyExportCompleted(export.UserId, export.FilePath);

                        _logger.LogInformation("Successfully processed export {ExportId}.", export.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing export {ExportId}.", export.Id);

                        // ثبت خطا در دیتابیس
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
