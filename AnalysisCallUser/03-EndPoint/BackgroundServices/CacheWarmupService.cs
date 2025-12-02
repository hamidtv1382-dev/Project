using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._03_EndPoint.Services;
using Microsoft.EntityFrameworkCore;

namespace AnalysisCallUser._03_EndPoint.BackgroundServices
{
    public class CacheWarmupService : IHostedService
    {
        private readonly ILogger<CacheWarmupService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public CacheWarmupService(ILogger<CacheWarmupService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Cache Warmup Service is starting.");

            // یک تأخیر کوچک برای اطمینان از اینکه سرویس‌های دیگر آماده هستند
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

            await WarmupCountriesCache(cancellationToken);
            await WarmupOperatorsCache(cancellationToken);
            await WarmupTopAnalyticsCache(cancellationToken);

            _logger.LogInformation("Cache Warmup Service has finished.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Cache Warmup Service is stopping.");
            return Task.CompletedTask;
        }

        private async Task WarmupCountriesCache(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var cacheKey = "AllCountries";
                var countries = await unitOfWork.Countries.GetAll().ToListAsync(cancellationToken);

                // کش کردن لیست کشورها برای ۱ روز
                await cacheService.GetOrCreateAsync(cacheKey, () => Task.FromResult(countries), TimeSpan.FromDays(1));
                _logger.LogInformation("Warmed up cache for countries.");
            }
        }

        private async Task WarmupOperatorsCache(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var cacheKey = "AllOperators";
                var operators = await unitOfWork.Operators.GetAll().ToListAsync(cancellationToken);

                // کش کردن لیست اپراتورها برای ۱ روز
                await cacheService.GetOrCreateAsync(cacheKey, () => Task.FromResult(operators), TimeSpan.FromDays(1));
                _logger.LogInformation("Warmed up cache for operators.");
            }
        }

        private async Task WarmupTopAnalyticsCache(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
                var callAnalysisService = scope.ServiceProvider.GetRequiredService<ICallAnalysisService>();

                var cacheKey = "Top5Countries";

                // کش کردن ۵ کشور برتر تماس برای ۱ ساعت
                await cacheService.GetOrCreateAsync(cacheKey, () => callAnalysisService.GetTopCallingCountriesAsync(5), TimeSpan.FromHours(1));
                _logger.LogInformation("Warmed up cache for top 5 countries.");
            }
        }
    }
}
