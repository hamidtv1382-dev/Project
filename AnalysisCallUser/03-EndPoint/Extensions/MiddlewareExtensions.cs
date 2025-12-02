using AnalysisCallUser._03_EndPoint.Middlewares;

namespace AnalysisCallUser._03_EndPoint.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomMiddlewares(this IApplicationBuilder app)
        {
            // ترتیب مهم است
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<PerformanceMiddleware>();
            app.UseMiddleware<ThemeMiddleware>();
            app.UseMiddleware<AuditMiddleware>();

            return app;
        }
    }
}
