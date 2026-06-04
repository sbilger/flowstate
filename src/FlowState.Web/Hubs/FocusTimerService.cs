using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace FlowState.Web.Hubs;

/// <summary>Tracks running focus sessions so the server can tick them authoritatively.</summary>
public class FocusTimerRegistry
{
    private record Entry(DateTimeOffset StartedAt, int PlannedSeconds, bool Paused, DateTimeOffset? PausedAt, int PausedSeconds);

    private readonly ConcurrentDictionary<Guid, Entry> _sessions = new();

    public void Start(Guid sessionId, DateTimeOffset startedAt, int plannedSeconds)
        => _sessions[sessionId] = new Entry(startedAt, plannedSeconds, false, null, 0);

    public void Stop(Guid sessionId) => _sessions.TryRemove(sessionId, out _);

    public void SetPaused(Guid sessionId, bool paused)
    {
        if (!_sessions.TryGetValue(sessionId, out var e)) return;
        if (paused && !e.Paused)
            _sessions[sessionId] = e with { Paused = true, PausedAt = DateTimeOffset.UtcNow };
        else if (!paused && e.Paused && e.PausedAt is not null)
        {
            var added = (int)(DateTimeOffset.UtcNow - e.PausedAt.Value).TotalSeconds;
            _sessions[sessionId] = e with { Paused = false, PausedAt = null, PausedSeconds = e.PausedSeconds + added };
        }
    }

    public IEnumerable<FocusTick> Snapshot(DateTimeOffset now)
    {
        foreach (var (id, e) in _sessions)
        {
            var raw = (int)(now - e.StartedAt).TotalSeconds - e.PausedSeconds;
            if (e.Paused && e.PausedAt is not null)
                raw -= (int)(now - e.PausedAt.Value).TotalSeconds;
            var elapsed = Math.Max(0, raw);
            yield return new FocusTick(id, elapsed, e.PlannedSeconds, elapsed > e.PlannedSeconds);
        }
    }
}

/// <summary>Broadcasts a tick to every running session's SignalR group once per second.</summary>
public class FocusTimerService : BackgroundService
{
    private readonly FocusTimerRegistry _registry;
    private readonly IHubContext<FocusHub> _hub;

    public FocusTimerService(FocusTimerRegistry registry, IHubContext<FocusHub> hub)
    {
        _registry = registry;
        _hub = hub;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var tick in _registry.Snapshot(now))
            {
                await _hub.Clients.Group(tick.SessionId.ToString())
                    .SendAsync("tick", tick, stoppingToken);
            }
        }
    }
}
