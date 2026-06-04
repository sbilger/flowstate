using FlowState.Domain.Focus;

namespace FlowState.Application.Common;

/// <summary>Persistence boundary for focus sessions.</summary>
public interface IFocusSessionRepository
{
    Task<FocusSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FocusSession?> GetActiveForTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FocusSession>> GetRecentAsync(int take, CancellationToken cancellationToken = default);
    Task AddAsync(FocusSession session, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
