using AnalysisCallUser._01_Domain.Core.Entities;

namespace AnalysisCallUser._01_Domain.Core.Contracts
{
    public interface IRealTimeService
    {
        // امضای متد با پیاده‌سازی مطابقت دارد
        Task SendLiveStatsUpdate(object stats);
        Task SendNewCallNotification(object newCall); // نوع ورودی با Entity هماهنگ نیست
        Task NotifyExportCompleted(int userId, string filePath);
    }
}
