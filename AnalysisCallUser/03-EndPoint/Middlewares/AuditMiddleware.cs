using System.Security.Claims;
using System.Text.Json;

namespace AnalysisCallUser._03_EndPoint.Middlewares
{

    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // فقط درخواست‌های POST, PUT, DELETE را لاگ کن
            if (context.Request.Method.Equals(HttpMethods.Post, StringComparison.OrdinalIgnoreCase) ||
                context.Request.Method.Equals(HttpMethods.Put, StringComparison.OrdinalIgnoreCase) ||
                context.Request.Method.Equals(HttpMethods.Delete, StringComparison.OrdinalIgnoreCase))
            {
                await LogAuditData(context);
            }

            await _next(context);
        }

        private async Task LogAuditData(HttpContext context)
        {
            context.Request.EnableBuffering();
            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var auditEntry = new
            {
                UserId = userId,
                Path = context.Request.Path,
                Method = context.Request.Method,
                QueryString = context.Request.QueryString.ToString(),
                RequestBody = body,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Audit Log: {AuditEntry}", JsonSerializer.Serialize(auditEntry));
        }
    }
}
