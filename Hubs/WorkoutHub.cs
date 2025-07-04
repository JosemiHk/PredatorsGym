using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace PredatorsGym.Hubs
{
    [Authorize]
    public class WorkoutHub : Hub
    {
        public async Task JoinWorkoutGroup(string routineId)
        {
            var groupName = $"workout_{routineId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("UserJoined", Context.UserIdentifier);
        }

        public async Task LeaveWorkoutGroup(string routineId)
        {
            var groupName = $"workout_{routineId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("UserLeft", Context.UserIdentifier);
        }

        public async Task SendWorkoutUpdate(string routineId, string message)
        {
            var groupName = $"workout_{routineId}";
            await Clients.Group(groupName).SendAsync("ReceiveWorkoutUpdate", message);
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Cleanup si es necesario
            await base.OnDisconnectedAsync(exception);
        }
    }
}
