using AnalysisCallUser._01_Domain.Core.Enums;

namespace AnalysisCallUser._03_EndPoint.Middlewares
{
    public class ThemeMiddleware
    {
        private readonly RequestDelegate _next;

        public ThemeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            const string themeKey = "UserTheme";
            var cookieTheme = context.Request.Cookies[themeKey];

            if (!string.IsNullOrEmpty(cookieTheme) && Enum.TryParse<ThemeMode>(cookieTheme, true, out var theme))
            {
                context.Items[themeKey] = theme;
            }
            else
            {
                // مقدار پیش‌فرض
                context.Items[themeKey] = ThemeMode.Light;
            }

            await _next(context);
        }
    }
}
