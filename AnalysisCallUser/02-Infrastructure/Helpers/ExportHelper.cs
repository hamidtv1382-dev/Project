using System.Text;

namespace AnalysisCallUser._02_Infrastructure.Helpers
{
    public static class ExportHelper
    {
        public static byte[] GenerateCsv<T>(IEnumerable<T> data)
        {
            if (data == null || !data.Any())
                return Array.Empty<byte>();

            var properties = typeof(T).GetProperties();
            var sb = new StringBuilder();

            // Add Header
            sb.AppendLine(string.Join(",", properties.Select(p => p.Name)));

            // Add Rows
            foreach (var item in data)
            {
                var values = properties.Select(p => GetValue(item, p));
                sb.AppendLine(string.Join(",", values));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string GetValue<T>(T item, System.Reflection.PropertyInfo prop)
        {
            var value = prop.GetValue(item, null);
            if (value == null) return "";

            // Handle commas in values by wrapping in quotes
            if (value is string stringValue && stringValue.Contains(","))
            {
                return $"\"{stringValue.Replace("\"", "\"\"")}\"";
            }

            return value.ToString();
        }
    }
}
