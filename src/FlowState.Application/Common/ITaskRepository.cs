using FlowState.Domain.Tasks;

namespace FlowState.Application.Common;

/// <summary>
/// Persistence boundary for tasks. Defined in Application (the dependency-inversion seam),
/// implemented in Infrastructure.
/// </summary>
public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskItem>> GetOpenAsync(CancellationToken cancellationToken = default);

    /// <summary>Open tasks, change-tracked, for the decay sweep (so transitions persist).</summary>
    Task<IReadOnlyList<TaskItem>> GetOpenTrackedAsync(CancellationToken cancellationToken = default);

    /// <summary>Tasks the resurfacing worker has flagged for a decision.</summary>
    Task<IReadOnlyList<TaskItem>> GetResurfacedAsync(CancellationToken cancellationToken = default);

    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);
    void Remove(TaskItem task);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
