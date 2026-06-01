namespace FlowState.Domain.Tasks;

/// <summary>
/// A single thing the user wants to do. The core entity of FlowState.
/// Slice 1 keeps this minimal; scoring/decay fields arrive in Slice 3.
/// </summary>
public class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public EnergyCost EnergyCost { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    // EF Core materialization constructor.
    private TaskItem()
    {
        Title = string.Empty;
    }

    public TaskItem(string title, EnergyCost energyCost)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title cannot be empty.", nameof(title));

        Id = Guid.NewGuid();
        Title = title.Trim();
        EnergyCost = energyCost;
        Status = TaskItemStatus.Open;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Complete()
    {
        if (Status == TaskItemStatus.Done) return;
        Status = TaskItemStatus.Done;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
