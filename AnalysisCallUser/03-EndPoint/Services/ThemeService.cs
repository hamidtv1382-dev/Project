using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._01_Domain.Core.Enums;
using static AnalysisCallUser._01_Domain.Core.DTOs.OperatorPerformanceDto;

namespace AnalysisCallUser._03_EndPoint.Services
{
    public interface IThemeService
    {
        Task<ThemeMode> GetUserThemeAsync(int userId);
        Task SetUserThemeAsync(int userId, ThemeMode theme);
    }

    public class ThemeService : IThemeService
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string ThemeCookieKey = "UserTheme";

        public ThemeService(IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ThemeMode> GetUserThemeAsync(int userId)
        {
            // ابتدا کوکی را چک می‌کنیم
            var cookieTheme = _httpContextAccessor.HttpContext.Request.Cookies[ThemeCookieKey];
            if (!string.IsNullOrEmpty(cookieTheme) && Enum.TryParse<ThemeMode>(cookieTheme, true, out var themeFromCookie))
            {
                return themeFromCookie;
            }

            // اگر کوکی نبود، از دیتابیس می‌خوانیم
            var user = await _userService.FindUserByIdAsync(userId);
            return user?.ThemePreference ?? ThemeMode.Light;
        }

        public async Task SetUserThemeAsync(int userId, ThemeMode theme)
        {
            // ذخیره در دیتابیس
            var user = await _userService.FindUserByIdAsync(userId);
            if (user != null)
            {
                user.ThemePreference = theme;
                await _userService.UpdateUserProfileAsync(new EditProfileDto { Id = userId, FirstName = user.FirstName, LastName = user.LastName });
            }

            // تنظیم کوکی
            var options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(30),
                IsEssential = true
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append(ThemeCookieKey, theme.ToString(), options);
        }
    }
}
