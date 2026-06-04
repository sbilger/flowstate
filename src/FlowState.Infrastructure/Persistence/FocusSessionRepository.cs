using FlowState.Application.Common;
using FlowState.Domain.Focus;
using Microsoft.EntityFrameworkCore;

namespace FlowState.Infrastructure.Persistence;

public class FocusSessionRepository : IFocusSessionRepository
{
    private readonly FlowStateDbContext _db;

    public FocusSessionRepository(FlowStateDbContext db) => _db = db;

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
}
