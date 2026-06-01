using FlowState.Domain.Tasks;

namespace FlowState.Application.Common;

/// <summary>
/// Persistence boundary for tasks. Defined in Application (the dependency-inversion seam),
/// implemented in Infrastructure.
/// </summary>
public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
