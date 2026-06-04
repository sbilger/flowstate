namespace FlowState.Domain.Tasks;

/// <summary>
/// A single thing the user wants to do. The core entity of FlowState.
/// </summary>
public class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public EnergyCost EnergyCost { get; private set; }
    public Importance Importance { get; private set; }
    public TaskItemStatus Status { get; private set; }

    /// <summary>How many times the user has hit "Not now" on this task. Feeds decay.</summary>
    public int SnoozeCount { get; private set; }

    /// <summary>When the task was last surfaced-and-dismissed; null until first snooze.</summary>
    public DateTimeOffset? LastSnoozedAt { get; private set; }

    /// <summary>Where the task sits in the decay/resurfacing lifecycle.</summary>
    public DecayState DecayState { get; private set; }

    /// <summary>
    /// Last meaningful interaction (created / acted on / edited / snoozed). Drives the
    /// freshness decay curve — an untouched task loses salience over time.
    /// </summary>
    public DateTimeOffset LastTouchedAt { get; private set; }

    /// <summary>When the task went Dormant; null while Active. Drives aging / anti-starvation.</summary>
    public DateTimeOffset? DormantSince { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    // EF Core materialization constructor.
    private TaskItem()
    {
        Title = string.Empty;
    }

    public TaskItem(string title, EnergyCost energyCost, Importance importance = Importance.Normal)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title cannot be empty.", nameof(title));

        var now = DateTimeOffset.UtcNow;
        Id = Guid.NewGuid();
        Title = title.Trim();
        EnergyCost = energyCost;
        Importance = importance;
        Status = TaskItemStatus.Open;
        SnoozeCount = 0;
        DecayState = DecayState.Active;
        CreatedAt = now;
        LastTouchedAt = now;
    }

    public void Complete()
    {
        if (Status == TaskItemStatus.Done) return;
        Status = TaskItemStatus.Done;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Reopen()
    {
        if (Status == TaskItemStatus.Open) return;
        Status = TaskItemStatus.Open;
        CompletedAt = null;
        WakeUp();
    }

    /// <summary>User dismissed the task from "What now?". Records avoidance for decay/resurfacing.</summary>
    public void Snooze()
    {
        SnoozeCount++;
        LastSnoozedAt = DateTimeOffset.UtcNow;
        Touch();
    }

    public void SetEnergyCost(EnergyCost energyCost) { EnergyCost = energyCost; Touch(); }
    public void SetImportance(Importance importance) { Importance = importance; Touch(); }

    // ---- decay / resurfacing lifecycle ----

    /// <summary>
    /// Records a meaningful interaction: refreshes the decay clock and returns the task to Active.
    /// Any genuine engagement "resets the rot".
    /// </summary>
    public void Touch()
    {
        LastTouchedAt = DateTimeOffset.UtcNow;
        WakeUp();
    }

    /// <summary>Return a task to the Active state (clears dormancy/resurfaced).</summary>
    public void WakeUp()
    {
        DecayState = DecayState.Active;
        DormantSince = null;
    }

    /// <summary>Move an Active task to Dormant (called by the decay worker). Idempotent.</summary>
    public void GoDormant(DateTimeOffset now)
    {
        if (DecayState == DecayState.Dormant) return;
        DecayState = DecayState.Dormant;
        DormantSince = now;
    }

    /// <summary>Force a Dormant task back as a decision (called by the resurfacing worker).</summary>
    public void Resurface()
    {
        DecayState = DecayState.Resurfaced;
    }
}
