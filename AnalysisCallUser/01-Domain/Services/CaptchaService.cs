using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;

namespace AnalysisCallUser._01_Domain.Services
{
    public interface ICaptchaService
    {
        string GenerateCaptchaCode(int length = 6);
        byte[] GenerateCaptchaImage(string captchaText, int width = 200, int height = 70);
        bool ValidateCaptcha(string userInput, string sessionCaptcha);
    }

    public class CaptchaService : ICaptchaService
    {
        private readonly string _allowedChars =
            "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";

        #region Generate Code

        public string GenerateCaptchaCode(int length = 6)
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);

            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(_allowedChars[bytes[i] % _allowedChars.Length]);
            }

            return sb.ToString();
        }

        #endregion

        #region Generate Image

        public byte[] GenerateCaptchaImage(string captchaText, int width = 200, int height = 70)
        {
            using var bitmap = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(bitmap);

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.White);

            AddBackgroundNoise(graphics, width, height);
            AddNoiseLines(graphics, width, height);
            DrawCaptchaText(graphics, captchaText, width, height);

            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }

        #endregion

        #region Noise

        private void AddBackgroundNoise(Graphics graphics, int width, int height)
        {
            Random rand = new Random();

            for (int i = 0; i < 120; i++)
            {
                int x = rand.Next(width);
                int y = rand.Next(height);

                using var brush = new SolidBrush(Color.FromArgb(
                    rand.Next(180, 230),
                    rand.Next(180, 230),
                    rand.Next(180, 230)));

                graphics.FillEllipse(brush, x, y, 2, 2);
            }
        }

        private void AddNoiseLines(Graphics graphics, int width, int height)
        {
            Random rand = new Random();

            for (int i = 0; i < 4; i++)
            {
                using var pen = new Pen(Color.LightGray, 1);

                graphics.DrawLine(
                    pen,
                    rand.Next(width), rand.Next(height),
                    rand.Next(width), rand.Next(height));
            }
        }

        #endregion

        #region Draw Text

        private void DrawCaptchaText(Graphics graphics, string text, int width, int height)
        {
            Random rand = new Random();

            using var font = new Font("Tahoma", 32, FontStyle.Bold);
            using var brush = new SolidBrush(Color.FromArgb(40, 40, 40));

            SizeF textSize = graphics.MeasureString(text, font);

            float startX = (width - textSize.Width) / 2;
            float startY = (height - textSize.Height) / 2;

            for (int i = 0; i < text.Length; i++)
            {
                float charX = startX + i * (textSize.Width / text.Length);
                float charY = startY + rand.Next(-3, 4);
                float angle = rand.Next(-12, 12);

                graphics.TranslateTransform(charX, charY);
                graphics.RotateTransform(angle);

                graphics.DrawString(
                    text[i].ToString(),
                    font,
                    brush,
                    0,
                    0);

                graphics.ResetTransform();
            }
        }

        #endregion

        #region Validation

        public bool ValidateCaptcha(string userInput, string sessionCaptcha)
        {
            return !string.IsNullOrWhiteSpace(userInput)
                   && !string.IsNullOrWhiteSpace(sessionCaptcha)
                   && userInput.Equals(sessionCaptcha, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}
