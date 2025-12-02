using System.Text;

namespace AnalysisCallUser._02_Infrastructure.Helpers
{
    public static class CacheHelper
    {
        public static string GenerateKey(string prefix, params object[] parameters)
        {
            var keyBuilder = new StringBuilder(prefix);
            foreach (var param in parameters)
            {
                keyBuilder.Append($"_{param}");
            }
            return keyBuilder.ToString();
        }
    }
}
