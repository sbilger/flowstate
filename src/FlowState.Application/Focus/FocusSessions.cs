using FlowState.Application.Common;
using FlowState.Domain.Focus;
using FlowState.Domain.Tasks;
using MediatR;

namespace FlowState.Application.Focus;

/// <summary>A focus session as exposed to the UI.</summary>
public record FocusSessionDto(
    Guid Id, Guid TaskId, string TaskTitle, DateTimeOffset StartedAt,
    int PlannedSeconds, int ElapsedSeconds, FocusOutcome Outcome)
{
    public static FocusSessionDto From(FocusSession s, DateTimeOffset now) =>
        new(s.Id, s.TaskId, s.TaskTitle, s.StartedAt,
            (int)s.PlannedDuration.TotalSeconds,
            (int)s.Elapsed(now).TotalSeconds,
            s.Outcome);
}

/// <summary>
/// Start a focus session on a task. Planned duration defaults from the task's energy cost
/// (low ≈ 5 min, medium ≈ 15, high ≈ 25 — Pomodoro-ish) unless overridden.
/// </summary>
public record StartFocusSessionCommand(Guid TaskId, int? PlannedMinutes = null) : IRequest<FocusSessionDto>;

public class StartFocusSessionHandler : IRequestHandler<StartFocusSessionCommand, FocusSessionDto>
{
    private readonly ITaskRepository _tasks;
    private readonly IFocusSessionRepository _sessions;

    public StartFocusSessionHandler(ITaskRepository tasks, IFocusSessionRepository sessions)
    {
        _tasks = tasks;
        _sessions = sessions;
    }

    public async Task<FocusSessionDto> Handle(StartFocusSessionCommand request, CancellationToken cancellationToken)
    {
        var task = await _tasks.GetByIdAsync(request.TaskId, cancellationToken)
            ?? throw new InvalidOperationException($"Task {request.TaskId} not found.");

        var minutes = request.PlannedMinutes ?? DefaultMinutes(task.EnergyCost);
        var session = new FocusSession(task.Id, task.Title, TimeSpan.FromMinutes(minutes));

        await _sessions.AddAsync(session, cancellationToken);
        await _sessions.SaveChangesAsync(cancellationToken);

        return FocusSessionDto.From(session, DateTimeOffset.UtcNow);
    }

    private static int DefaultMinutes(EnergyCost cost) => cost switch
    {
        EnergyCost.High => 25,
        EnergyCost.Medium => 15,
        _ => 5
    };
}

/// <summary>
/// End a focus session with an outcome. If completed, the underlying task is marked done;
/// otherwise the task is "touched" (refreshes its decay clock — engaging with it counts).
/// </summary>
public record EndFocusSessionCommand(Guid SessionId, FocusOutcome Outcome) : IRequest<bool>;

public class EndFocusSessionHandler : IRequestHandler<EndFocusSessionCommand, bool>
{
    private readonly ITaskRepository _tasks;
    private readonly IFocusSessionRepository _sessions;

    public EndFocusSessionHandler(ITaskRepository tasks, IFocusSessionRepository sessions)
    {
        _tasks = tasks;
        _sessions = sessions;
    }

    public async Task<bool> Handle(EndFocusSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessions.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null) return false;

        session.End(request.Outcome);
        await _sessions.SaveChangesAsync(cancellationToken);

        var task = await _tasks.GetByIdAsync(session.TaskId, cancellationToken);
        if (task is not null)
        {
            if (request.Outcome == FocusOutcome.Completed)
                task.Complete();
            else
                task.Touch(); // engaging with it refreshes the decay clock
            await _tasks.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
