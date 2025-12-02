using AnalysisCallUser._01_Domain.Core.Contracts;
using AnalysisCallUser._03_EndPoint.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AnalysisCallUser._03_EndPoint.Services
{
    public class RealTimeService : IRealTimeService
    {
        private readonly IHubContext<LiveDataHub> _liveDataHub;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public RealTimeService(IHubContext<LiveDataHub> liveDataHub, IHubContext<NotificationHub> notificationHub)
        {
            _liveDataHub = liveDataHub;
            _notificationHub = notificationHub;
        }

        public async Task SendLiveStatsUpdate(object stats)
        {
            await _liveDataHub.Clients.All.SendAsync("ReceiveLiveStats", stats);
        }

        public async Task SendNewCallNotification(object newCall)
        {
            await _liveDataHub.Clients.All.SendAsync("ReceiveNewCall", newCall);
        }

        public async Task NotifyExportCompleted(int userId, string filePath)
        {
            await _notificationHub.Clients.User(userId.ToString()).SendAsync("ExportCompleted", filePath);
        }
    }
}
