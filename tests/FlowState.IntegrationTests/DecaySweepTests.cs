using FlowState.Application.Tasks;
using FlowState.Domain.Tasks;
using FlowState.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace FlowState.IntegrationTests;

/// <summary>
/// Verifies the decay sweep persists lifecycle transitions through real Postgres,
/// and that resurfaced tasks surface in the decision query but not in "What now?".
/// </summary>
public class DecaySweepTests : IAsyncLifetime
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

    [Fact]
    public async Task Sweep_PersistsDormantTransition_ForSnoozedTask()
    {
        var repo = new TaskRepository(_factory);
        var task = new TaskItem("avoided", EnergyCost.Low);
        task.Snooze(); task.Snooze(); task.Snooze(); // past snooze threshold → eligible for dormant
        await repo.AddAsync(task);
        await repo.SaveChangesAsync();

        var sweep = new RunDecaySweepHandler(new TaskRepository(_factory));
        var result = await sweep.Handle(new RunDecaySweepCommand(), CancellationToken.None);

        Assert.True(result.WentDormant >= 1);

        var reloaded = await new TaskRepository(_factory).GetByIdAsync(task.Id);
        Assert.Equal(DecayState.Dormant, reloaded!.DecayState);
        Assert.NotNull(reloaded.DormantSince);
    }

    [Fact]
    public async Task ResurfacedTask_AppearsInDecisionQuery_NotInWhatNow()
    {
        var repo = new TaskRepository(_factory);
        var task = new TaskItem("resurface me", EnergyCost.Low, Importance.High);
        task.GoDormant(DateTimeOffset.UtcNow.AddDays(-30)); // long dormant
        await repo.AddAsync(task);
        await repo.SaveChangesAsync();

        // Sweep should resurface it (past hard ceiling).
        await new RunDecaySweepHandler(new TaskRepository(_factory))
            .Handle(new RunDecaySweepCommand(), CancellationToken.None);

        var resurfaced = await new GetResurfacedHandler(new TaskRepository(_factory))
            .Handle(new GetResurfacedQuery(), CancellationToken.None);
        Assert.Contains(resurfaced, r => r.Task.Id == task.Id);

        var whatNow = await new GetWhatNowHandler(new TaskRepository(_factory))
            .Handle(new GetWhatNowQuery(EnergyLevel.Good), CancellationToken.None);
        Assert.NotEqual(task.Id, whatNow.Now?.Task.Id);
    }

    [Fact]
    public async Task KeepCommand_ReturnsResurfacedTask_ToActive()
    {
        var repo = new TaskRepository(_factory);
        var task = new TaskItem("keep me", EnergyCost.Low, Importance.High);
        task.GoDormant(DateTimeOffset.UtcNow.AddDays(-30));
        await repo.AddAsync(task);
        await repo.SaveChangesAsync();

        await new RunDecaySweepHandler(new TaskRepository(_factory))
            .Handle(new RunDecaySweepCommand(), CancellationToken.None);
        await new KeepTaskHandler(new TaskRepository(_factory))
            .Handle(new KeepTaskCommand(task.Id), CancellationToken.None);

        var reloaded = await new TaskRepository(_factory).GetByIdAsync(task.Id);
        Assert.Equal(DecayState.Active, reloaded!.DecayState);
    }
}
