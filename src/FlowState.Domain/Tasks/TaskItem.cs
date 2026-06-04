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

        Id = Guid.NewGuid();
        Title = title.Trim();
        EnergyCost = energyCost;
        Importance = importance;
        Status = TaskItemStatus.Open;
        SnoozeCount = 0;
        CreatedAt = DateTimeOffset.UtcNow;
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
    }

    /// <summary>User dismissed the task from "What now?". Records avoidance for decay/resurfacing.</summary>
    public void Snooze()
    {
        SnoozeCount++;
        LastSnoozedAt = DateTimeOffset.UtcNow;
    }

    public void SetEnergyCost(EnergyCost energyCost) => EnergyCost = energyCost;
    public void SetImportance(Importance importance) => Importance = importance;
}
