using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace AnalysisCallUser._03_EndPoint.Hubs
{
    [Authorize]
    public class AnalyticsHub : Hub
    {
        // کاربر برای مشاهده تحلیل‌های یک کشور خاص، به گروه آن کشور ملحق می‌شود
        public async Task SubscribeToCountryUpdates(int countryId)
        {
            string groupName = $"Country_{countryId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("Subscribed", $"You are now subscribed to updates for country {countryId}.");
        }

        // کاربر برای لغو اشتراک تحلیل‌های یک کشور
        public async Task UnsubscribeFromCountryUpdates(int countryId)
        {
            string groupName = $"Country_{countryId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("Unsubscribed", $"You have unsubscribed from updates for country {countryId}.");
        }

        // کاربر برای مشاهده تحلیل‌های یک اپراتور خاص
        public async Task SubscribeToOperatorUpdates(int operatorId)
        {
            string groupName = $"Operator_{operatorId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // کاربر برای لغو اشتراک تحلیل‌های یک اپراتور
        public async Task UnsubscribeFromOperatorUpdates(int operatorId)
        {
            string groupName = $"Operator_{operatorId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
