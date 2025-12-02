using System.Diagnostics;

namespace AnalysisCallUser._03_EndPoint.Middlewares
{
    public class PerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMiddleware> _logger;
        private readonly long _thresholdMilliseconds;

        public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger, long thresholdMilliseconds = 1000)
        {
            _next = next;
            _logger = logger;
            _thresholdMilliseconds = thresholdMilliseconds;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            await _next(context);
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > _thresholdMilliseconds)
            {
                _logger.LogWarning("Slow request detected: {Method} {Path} took {ElapsedMilliseconds}ms",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
