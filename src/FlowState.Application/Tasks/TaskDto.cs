using FlowState.Domain.Tasks;

namespace FlowState.Application.Tasks;

/// <summary>A task as exposed to the UI/API.</summary>
public record TaskDto(
    Guid Id,
    string Title,
    EnergyCost EnergyCost,
    Importance Importance,
    TaskItemStatus Status,
    int SnoozeCount,
    DateTimeOffset CreatedAt)
{
    public static TaskDto From(TaskItem t) =>
        new(t.Id, t.Title, t.EnergyCost, t.Importance, t.Status, t.SnoozeCount, t.CreatedAt);
}

/// <summary>A surfaced task plus its score and a human reason, for "What now?".</summary>
public record SurfacedTaskDto(TaskDto Task, double Score, string Reason);
