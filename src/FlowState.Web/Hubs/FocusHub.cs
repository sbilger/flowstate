using Microsoft.AspNetCore.SignalR;

namespace FlowState.Web.Hubs;

/// <summary>
/// SignalR hub for live focus timers. The server is the source of truth: a hosted service
/// broadcasts ticks every second to the session's group, so the timer survives client
/// refreshes/reconnects and can't drift. Clients join a per-session group.
/// </summary>
public class FocusHub : Hub
{
    public async Task JoinSession(Guid sessionId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, sessionId.ToString());

    public async Task LeaveSession(Guid sessionId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId.ToString());
}

/// <summary>Payload pushed to clients each tick.</summary>
public record FocusTick(Guid SessionId, int ElapsedSeconds, int PlannedSeconds, bool Overtime);
