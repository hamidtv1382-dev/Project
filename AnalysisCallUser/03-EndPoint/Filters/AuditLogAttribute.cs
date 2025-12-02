using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Text.Json;

namespace AnalysisCallUser._03_EndPoint.Filters
{
    public class AuditLogAttribute : ActionFilterAttribute
    {
        private readonly ILogger<AuditLogAttribute> _logger;

        public AuditLogAttribute(ILogger<AuditLogAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var actionName = context.ActionDescriptor.RouteValues["action"];
            var controllerName = context.ActionDescriptor.RouteValues["controller"];

            var auditInfo = new
            {
                UserId = userId,
                Controller = controllerName,
                Action = actionName,
                Timestamp = DateTime.UtcNow,
                Succeeded = context.Exception == null
            };

            _logger.LogInformation("Action Audit: {AuditInfo}", JsonSerializer.Serialize(auditInfo));
        }
    }
}
