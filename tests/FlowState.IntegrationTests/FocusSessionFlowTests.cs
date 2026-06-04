using FlowState.Application.Focus;
using FlowState.Domain.Focus;
using FlowState.Domain.Tasks;
using FlowState.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace FlowState.IntegrationTests;

/// <summary>
/// Verifies the focus-session flow through real Postgres: starting a session derives a
/// planned duration, and ending it completes or touches the underlying task.
/// </summary>
public class FocusSessionFlowTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private FlowStateDbContext _db = null!;
    private TestDbContextFactory _factory = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        var options = new DbContextOptionsBuilder<FlowStateDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        _db = new FlowStateDbContext(options);
        _factory = new TestDbContextFactory(_postgres.GetConnectionString());
        await _db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    private async Task<TaskItem> SeedTask(EnergyCost cost)
    {
        var repo = new TaskRepository(_factory);
        var task = new TaskItem("focus me", cost, Importance.High);
        await repo.AddAsync(task);
        await repo.SaveChangesAsync();
        return task;
    }

    [Fact]
    public async Task Start_DerivesPlannedDuration_FromEnergyCost()
    {
        var task = await SeedTask(EnergyCost.High); // → 25 min default
        var start = new StartFocusSessionHandler(new TaskRepository(_factory), new FocusSessionRepository(_factory));

        var session = await start.Handle(new StartFocusSessionCommand(task.Id), CancellationToken.None);

        Assert.Equal(25 * 60, session.PlannedSeconds);
        Assert.Equal(FocusOutcome.InProgress, session.Outcome);
    }

    [Fact]
    public async Task End_Completed_MarksTaskDone()
    {
        var task = await SeedTask(EnergyCost.Low);
        var start = new StartFocusSessionHandler(new TaskRepository(_factory), new FocusSessionRepository(_factory));
        var end = new EndFocusSessionHandler(new TaskRepository(_factory), new FocusSessionRepository(_factory));

        var session = await start.Handle(new StartFocusSessionCommand(task.Id), CancellationToken.None);
        await end.Handle(new EndFocusSessionCommand(session.Id, FocusOutcome.Completed), CancellationToken.None);

        var reloaded = await new TaskRepository(_factory).GetByIdAsync(task.Id);
        Assert.Equal(TaskItemStatus.Done, reloaded!.Status);
    }

    [Fact]
    public async Task End_Progress_TouchesTask_ButKeepsItOpen()
    {
        var task = await SeedTask(EnergyCost.Medium);
        var start = new StartFocusSessionHandler(new TaskRepository(_factory), new FocusSessionRepository(_factory));
        var end = new EndFocusSessionHandler(new TaskRepository(_factory), new FocusSessionRepository(_factory));

        var session = await start.Handle(new StartFocusSessionCommand(task.Id), CancellationToken.None);
        await end.Handle(new EndFocusSessionCommand(session.Id, FocusOutcome.Progress), CancellationToken.None);

        var reloaded = await new TaskRepository(_factory).GetByIdAsync(task.Id);
        Assert.Equal(TaskItemStatus.Open, reloaded!.Status);
        Assert.Equal(DecayState.Active, reloaded.DecayState); // touch keeps it active
    }
}
