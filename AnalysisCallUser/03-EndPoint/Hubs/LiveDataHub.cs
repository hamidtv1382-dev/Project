using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace AnalysisCallUser._03_EndPoint.Hubs
{
    [Authorize] // فقط کاربران احراز هویت شده می‌توانند متصل شوند
    public class LiveDataHub : Hub
    {
        private static int _connectedUsersCount = 0;

        public override async Task OnConnectedAsync()
        {
            _connectedUsersCount++;
            await Clients.All.SendAsync("UpdateUserCount", _connectedUsersCount);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _connectedUsersCount--;
            await Clients.All.SendAsync("UpdateUserCount", _connectedUsersCount);
            await base.OnDisconnectedAsync(exception);
        }

        // کلاینت می‌تواند برای عضویت در گروه‌های خاص این متد را فراخوانی کند
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
