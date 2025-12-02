using AnalysisCallUser._02_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AnalysisCallUser._03_EndPoint.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(int userId, string title, string message, string link = null);
        Task<IEnumerable<UserNotification>> GetUserNotificationsAsync(int userId);
        Task MarkAsReadAsync(int notificationId, int userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(int userId, string title, string message, string link = null)
        {
            var notification = new UserNotification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Link = link,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            await _context.Set<UserNotification>().AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserNotification>> GetUserNotificationsAsync(int userId)
        {
            return await _context.Set<UserNotification>()
                                .Where(n => n.UserId == userId)
                                .OrderByDescending(n => n.CreatedAt)
                                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Set<UserNotification>()
                                           .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }

    // کلاس موجودیت نوتیفیکیشن (در لایه Domain تعریف می‌شود)
    public class UserNotification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Link { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
