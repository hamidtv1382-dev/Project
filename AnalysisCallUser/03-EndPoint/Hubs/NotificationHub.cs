using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AnalysisCallUser._03_EndPoint.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        // وقتی کاربر متصل می‌شود، می‌توانیم اطلاع‌رسانی کنیم که آماده دریافت نوتیفیکیشن‌هاست
        public override async Task OnConnectedAsync()
        {
            // مثال: ارسال پیام خوش‌آمدگویی به کاربر
            await Clients.Caller.SendAsync("ReceiveNotification", "Welcome! You are now connected for notifications.");
            await base.OnConnectedAsync();
        }

        // کلاینت می‌تواند وضعیت یک نوتیفیکیشن را به "خوانده شده" تغییر دهد
        public async Task MarkNotificationAsRead(int notificationId)
        {
            // در اینجا می‌توانید منطق مربوط به به‌روزرسانی وضعیت نوتیفیکیشن در دیتابیس را فراخوانی کنید
            // مثلا: await _notificationService.MarkAsReadAsync(notificationId);

            // ارسال تأیید به کلاینت که وضعیت تغییر کرده است
            await Clients.Caller.SendAsync("NotificationRead", notificationId);
        }
    }
}
