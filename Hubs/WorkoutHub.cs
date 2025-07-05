using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace PredatorsGym.Hubs
{
    [Authorize]
    public class WorkoutHub : Hub
    {
        private readonly ILogger<WorkoutHub> _logger;

        public WorkoutHub(ILogger<WorkoutHub> logger)
        {
            _logger = logger;
        }

        public async Task JoinWorkoutGroup(string routineId)
        {
            try
            {
                var groupName = $"workout_{routineId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                _logger.LogInformation("Usuario {UserId} se unió al grupo {GroupName}",
                    Context.UserIdentifier, groupName);

                await Clients.Group(groupName).SendAsync("UserJoined", Context.UserIdentifier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al unirse al grupo {RouteId}", routineId);
                throw;
            }
        }

        public async Task LeaveWorkoutGroup(string routineId)
        {
            try
            {
                var groupName = $"workout_{routineId}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

                _logger.LogInformation("Usuario {UserId} salió del grupo {GroupName}",
                    Context.UserIdentifier, groupName);

                await Clients.Group(groupName).SendAsync("UserLeft", Context.UserIdentifier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al salir del grupo {RouteId}", routineId);
                throw;
            }
        }

        public async Task SendWorkoutUpdate(string routineId, string message)
        {
            var groupName = $"workout_{routineId}";
            await Clients.Group(groupName).SendAsync("ReceiveWorkoutUpdate", message);
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Cliente conectado: {ConnectionId}, Usuario: {UserId}",
                Context.ConnectionId, Context.UserIdentifier);

            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Cliente desconectado: {ConnectionId}, Usuario: {UserId}, Excepción: {Exception}",
                Context.ConnectionId, Context.UserIdentifier, exception?.Message);

            await base.OnDisconnectedAsync(exception);
        }
    }
}