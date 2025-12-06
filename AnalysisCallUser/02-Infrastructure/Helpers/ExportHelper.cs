using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

public static class ExportHelper
{
    /// <summary>
    /// داده‌های یک لیست را به فرمت CSV تبدیل می‌کند.
    /// </summary>
    /// <typeparam name="T">نوع داده‌های موجود در لیست</typeparam>
    /// <param name="data">لیستی از داده‌ها برای اکسپورت</param>
    /// <param name="selectedColumns">لیست نام ستون‌هایی که باید اکسپورت شوند. اگر null یا خالی باشد، تمام ستون‌ها اکسپورت می‌شوند.</param>
    /// <returns>آرایه‌ای از بایت‌ها که محتوای فایل CSV است.</returns>
    public static byte[] GenerateCsv<T>(IEnumerable<T> data, List<string> selectedColumns = null)
    {
        if (data == null || !data.Any())
            return Array.Empty<byte>();

        var allProperties = typeof(T).GetProperties();
        IEnumerable<PropertyInfo> propertiesToExport;

        // اگر ستونی انتخاب نشده باشد، تمام پراپرتی‌ها را در نظر بگیر
        if (selectedColumns == null || !selectedColumns.Any())
        {
            propertiesToExport = allProperties;
        }
        else
        {
            // فقط پراپرتی‌هایی را انتخاب کن که نامشان در لیست انتخاب شده باشد
            // از StringComparison.OrdinalIgnoreCase برای عدم حساسیت به بزرگی و کوچکی حروف استفاده می‌کنیم
            propertiesToExport = allProperties.Where(p => selectedColumns.Contains(p.Name, StringComparer.OrdinalIgnoreCase));
        }

        // اگر پس از فیلتر کردن، هیچ پراپرتی‌ای باقی نماند، یک فایل خالی برگردان
        if (!propertiesToExport.Any())
        {
            return Array.Empty<byte>();
        }

        var properties = propertiesToExport.ToArray();
        var sb = new StringBuilder();

        // افزودن هدر با نام‌های فارسی برای خوانایی بهتر
        sb.AppendLine(string.Join(",", GetPersianHeaders(properties)));

        // افزودن ردیف‌های داده
        foreach (var item in data)
        {
            var values = properties.Select(p => GetValue(item, p));
            sb.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    /// <summary>
    /// نام پراپرتی‌ها را به هدرهای فارسی معادل نگاشت می‌کند.
    /// </summary>
    /// <param name="properties">آرایه‌ای از اطلاعات پراپرتی‌ها</param>
    /// <returns>آرایه‌ای از هدرهای فارسی</returns>
    private static string[] GetPersianHeaders(PropertyInfo[] properties)
    {
        // نگاشت نام پراپرتی‌ها به هدرهای فارسی
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "DetailID", "شناسه تماس" },
            { "ANumber", "شماره مبدأ" },
            { "BNumber", "شماره مقصد" },
            { "AccountingTime", "تاریخ و زمان" },
            { "Length", "مدت (ثانیه)" },
            { "OriginCountryName", "کشور مبدأ" },
            { "OriginCityName", "شهر مبدأ" },
            { "DestCountryName", "کشور مقصد" },
            { "DestCityName", "شهر مقصد" },
            { "OriginOperatorName", "اپراتور مبدأ" },
            { "DestOperatorName", "اپراتور مقصد" },
            { "Answer", "وضعیت" }
        };

        return properties.Select(p =>
            headers.ContainsKey(p.Name) ? headers[p.Name] : p.Name
        ).ToArray();
    }

    /// <summary>
    /// مقدار یک پراپرتی از یک آبجکت را استخراج و فرمت می‌کند.
    /// </summary>
    /// <typeparam name="T">نوع آبجکت</typeparam>
    /// <param name="item">آبجکت مورد نظر</param>
    /// <param name="prop">اطلاعات پراپرتی</param>
    /// <returns>مقدار فرمت‌شده به صورت رشته</returns>
    private static string GetValue<T>(T item, PropertyInfo prop)
    {
        var value = prop.GetValue(item, null);
        if (value == null) return "";

        // فرمت‌بندی تاریخ و زمان برای فرهنگ فارسی (یا اینواریانت)
        if (value is DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        // فرمت‌بندی مقادیر بولین
        if (value is bool boolValue)
        {
            return boolValue ? "پاسخ داده شده" : "پاسخ داده نشده";
        }

        // مدیریت کاما در مقادیر رشته‌ای با قرار دادن آن‌ها در کوتیشن
        // و جایگزینی کوتیشن‌های داخلی با دو کوتیشن
        if (value is string stringValue && stringValue.Contains(","))
        {
            return $"\"{stringValue.Replace("\"", "\"\"")}\"";
        }

        return value.ToString();
    }
}