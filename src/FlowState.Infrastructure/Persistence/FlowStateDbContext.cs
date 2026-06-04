using System.Reflection;
using FlowState.Domain.Focus;
using FlowState.Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FlowState.Infrastructure.Persistence;

public class FlowStateDbContext : DbContext
{
    public FlowStateDbContext(DbContextOptions<FlowStateDbContext> options) : base(options) { }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<FocusSession> FocusSessions => Set<FocusSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
