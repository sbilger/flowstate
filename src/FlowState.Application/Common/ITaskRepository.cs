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
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);
    void Remove(TaskItem task);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
