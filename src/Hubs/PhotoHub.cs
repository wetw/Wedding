using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Wedding.Hubs
{
    public class PhotoHub : Hub
    {
        private const string GroupName = "photo";

        public async Task SendMessage(string user, string message)
        {
            await Clients.Group(GroupName).SendAsync("ReceiveMessage", user, message).ConfigureAwait(false);
        }

        public async Task Subscribe()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName).ConfigureAwait(false);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName).ConfigureAwait(false);
            await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
        }
    }
}
