using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Arif.Platform.LiveAgent.Infrastructure.Hubs
{
    public class AgentHub : Hub
    {
        private readonly ILogger<AgentHub> _logger;

        public AgentHub(ILogger<AgentHub> logger)
        {
            _logger = logger;
        }

        public async Task JoinAgentGroup(string agentId)
        {
            _logger.LogInformation("Agent {AgentId} joining group with connection {ConnectionId}", agentId, Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"agent_{agentId}");
        }

        public async Task LeaveAgentGroup(string agentId)
        {
            _logger.LogInformation("Agent {AgentId} leaving group with connection {ConnectionId}", agentId, Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"agent_{agentId}");
        }

        public async Task SendMessageToAgent(string agentId, string message)
        {
            _logger.LogInformation("Sending message to agent {AgentId}: {Message}", agentId, message);
            await Clients.Group($"agent_{agentId}").SendAsync("ReceiveMessage", message);
        }

        public async Task UpdateAgentStatus(string agentId, string status)
        {
            _logger.LogInformation("Agent {AgentId} updating status to {Status}", agentId, status);
            await Clients.Others.SendAsync("AgentStatusUpdated", agentId, status);
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Agent client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Agent client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
