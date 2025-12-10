using System.Globalization;

namespace AnalysisCallUser._02_Infrastructure.Helpers
{
    public static class PersianDateHelper
    {
        /// <summary>
        /// تلاش می‌کند یک رشته تاریخ شمسی را به DateTime میلادی تبدیل کند.
        /// در صورت موفقیت DateTime را برمی‌گرداند، در غیر این‌صورت null.
        /// پشتیبانی از جداکننده‌های '/', '-', '.' و ارقام فارسی.
        /// </summary>
        public static DateTime? ToGregorian(string persianDate)
        {
            if (string.IsNullOrWhiteSpace(persianDate))
                return null;

            // پاکسازی و تبدیل ارقام فارسی به انگلیسی
            var normalized = ConvertPersianDigitsToEnglish(persianDate).Trim();

            // اگر همراه با زمان باشد (مثلاً "1403/10/05 13:20:00") فقط قسمت تاریخ را می‌گیریم
            var datePart = normalized.Split(' ')[0];

            // جداکننده‌ها: / - .
            var parts = datePart.Split(new[] { '/', '-', '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3)
                return null;

            if (!int.TryParse(parts[0], out int year)) return null;
            if (!int.TryParse(parts[1], out int month)) return null;
            if (!int.TryParse(parts[2], out int day)) return null;

            try
            {
                var pc = new PersianCalendar();
                // ساعت/دقیقه/ثانیه را 0 قرار می‌دهیم — اگر لازم داشتید می‌توانید پارس کردن زمان را هم اضافه کنید
                var dt = pc.ToDateTime(year, month, day, 0, 0, 0, 0);
                return dt;
            }
            catch
            {
                return null;
            }
        }

        private static string ConvertPersianDigitsToEnglish(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // نگاشت ارقام فارسی به انگلیسی
            var persianDigits = new[] { '۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹' };
            var arabicDigits = new[] { '٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩' };

            var chars = input.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                for (int d = 0; d < 10; d++)
                {
                    if (chars[i] == persianDigits[d] || chars[i] == arabicDigits[d])
                    {
                        chars[i] = (char)('0' + d);
                        break;
                    }
                }
            }

            return new string(chars);
        }
    }
}
