using FlowState.Application.Common;
using FlowState.Domain.Focus;
using Microsoft.EntityFrameworkCore;

namespace FlowState.Infrastructure.Persistence;

public class FocusSessionRepository : IFocusSessionRepository, IDisposable
{
    private readonly FlowStateDbContext _db;

    /// <summary>Each repository instance owns a fresh context from the factory (see TaskRepository).</summary>
    public FocusSessionRepository(IDbContextFactory<FlowStateDbContext> factory)
        => _db = factory.CreateDbContext();

    public async Task<FocusSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _db.FocusSessions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<FocusSession?> GetActiveForTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
        => await _db.FocusSessions
            .Where(s => s.TaskId == taskId && s.Outcome == FocusOutcome.InProgress)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<FocusSession>> GetRecentAsync(int take, CancellationToken cancellationToken = default)
        => await _db.FocusSessions.AsNoTracking()
            .OrderByDescending(s => s.StartedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(FocusSession session, CancellationToken cancellationToken = default)
        => await _db.FocusSessions.AddAsync(session, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _db.SaveChangesAsync(cancellationToken);

    public void Dispose() => _db.Dispose();
}
