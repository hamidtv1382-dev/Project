using System.Reflection;

namespace AnalysisCallUser._03_EndPoint.Extensions
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// یک آبجکت را به دیکشنری تبدیل می‌کند. فقط پراپرتی‌هایی که مقدار غیر null و غیر پیش‌فرض دارند، اضافه می‌شوند.
        /// </summary>
        public static Dictionary<string, string> ToDictionary(this object obj)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (obj == null) return dictionary;

            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var value = property.GetValue(obj, null);

                // اگر مقدار null بود یا مقدار پیش‌فرض نوع داده بود، آن را نادیده بگیر
                if (value == null || IsDefaultValue(value))
                {
                    continue;
                }

                // اگر مقدار یک enum بود، آن را به رشته تبدیل کن
                if (property.PropertyType.IsEnum)
                {
                    dictionary.Add(property.Name, value.ToString());
                }
                // برای انواع دیگر، مقدار را به رشته تبدیل کن
                else
                {
                    dictionary.Add(property.Name, value.ToString());
                }
            }

            return dictionary;
        }

        private static bool IsDefaultValue(object value)
        {
            if (value == null) return true;

            Type type = value.GetType();

            // برای انواع مقداری (Value Types)
            if (type.IsValueType)
            {
                return Equals(value, Activator.CreateInstance(type));
            }

            // برای رشته‌ها
            if (value is string stringValue)
            {
                return string.IsNullOrEmpty(stringValue);
            }

            return false;
        }
    }
}
