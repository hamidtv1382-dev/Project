namespace AnalysisCallUser._03_EndPoint.Extensions
{
    public static class ConfigurationExtensions
    {
        public static string GetDefaultConnectionString(this IConfiguration configuration)
        {
            return configuration.GetConnectionString("DefaultConnection")
                   ?? throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        public static JwtSettings GetJwtSettings(this IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            if (!jwtSettings.Exists())
            {
                throw new InvalidOperationException("JwtSettings section is not configured.");
            }

            return new JwtSettings
            {
                SecretKey = jwtSettings["SecretKey"],
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                ExpiryInMinutes = Convert.ToInt32(jwtSettings["ExpiryInMinutes"] ?? "60")
            };
        }
    }

    // کلاس کمکی برای نگهداری تنظیمات JWT
    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpiryInMinutes { get; set; }
    }
}
