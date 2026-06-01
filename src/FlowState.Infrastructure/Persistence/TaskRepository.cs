using FlowState.Application.Common;
using FlowState.Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FlowState.Infrastructure.Persistence;

public class TaskRepository : ITaskRepository
{
    private readonly FlowStateDbContext _db;

    public TaskRepository(FlowStateDbContext db) => _db = db;

    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Tasks.AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
        => await _db.Tasks.AddAsync(task, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _db.SaveChangesAsync(cancellationToken);
}
