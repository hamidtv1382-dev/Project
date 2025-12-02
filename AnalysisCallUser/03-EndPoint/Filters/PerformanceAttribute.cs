using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace AnalysisCallUser._03_EndPoint.Filters
{
    public class PerformanceAttribute : ActionFilterAttribute
    {
        private readonly ILogger<PerformanceAttribute> _logger;
        private Stopwatch _stopwatch;

        public PerformanceAttribute(ILogger<PerformanceAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            _stopwatch.Stop();
            var actionName = context.ActionDescriptor.DisplayName;
            var elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;

            _logger.LogInformation("Action '{ActionName}' executed in {ElapsedMilliseconds}ms.", actionName, elapsedMilliseconds);
        }
    }
}
