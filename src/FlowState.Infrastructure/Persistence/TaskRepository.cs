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

    public async Task<IReadOnlyList<TaskItem>> GetOpenAsync(CancellationToken cancellationToken = default)
        => await _db.Tasks.AsNoTracking()
            .Where(t => t.Status == TaskItemStatus.Open)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
        => await _db.Tasks.AddAsync(task, cancellationToken);

    public void Remove(TaskItem task) => _db.Tasks.Remove(task);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _db.SaveChangesAsync(cancellationToken);
}
