using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AssignmentApi.Hubs
{
    public class RideHub : Hub
    {
        // Clients call this to receive real-time notifications sent to their userId group
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }
    }
}
