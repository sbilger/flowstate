namespace FlowState.Domain.Focus;

/// <summary>
/// A focus session on a single task: when it started, how long was planned, when it ended,
/// and the outcome. Persisted so Insights (Slice 5) can learn peak windows + session length.
/// The live ticking is server-authoritative via SignalR; this entity is the durable record.
/// </summary>
public class FocusSession
{
    public Guid Id { get; private set; }
    public Guid TaskId { get; private set; }
    public string TaskTitle { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }

    /// <summary>Planned duration (the countdown target). Drives the visual ring.</summary>
    public TimeSpan PlannedDuration { get; private set; }

    public FocusOutcome Outcome { get; private set; }

    // EF materialization
    private FocusSession()
    {
        TaskTitle = string.Empty;
    }

    public FocusSession(Guid taskId, string taskTitle, TimeSpan plannedDuration)
    {
        if (plannedDuration <= TimeSpan.Zero)
            throw new ArgumentException("Planned duration must be positive.", nameof(plannedDuration));

        Id = Guid.NewGuid();
        TaskId = taskId;
        TaskTitle = taskTitle;
        PlannedDuration = plannedDuration;
        StartedAt = DateTimeOffset.UtcNow;
        Outcome = FocusOutcome.InProgress;
    }

    /// <summary>Actual elapsed time (live while running, frozen once ended).</summary>
    public TimeSpan Elapsed(DateTimeOffset now) => (EndedAt ?? now) - StartedAt;

    public void End(FocusOutcome outcome)
    {
        if (Outcome != FocusOutcome.InProgress) return;
        Outcome = outcome == FocusOutcome.InProgress ? FocusOutcome.Abandoned : outcome;
        EndedAt = DateTimeOffset.UtcNow;
    }
}
