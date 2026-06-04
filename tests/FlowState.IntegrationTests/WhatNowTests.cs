using FlowState.Application.Tasks;
using FlowState.Domain.Tasks;
using FlowState.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace FlowState.IntegrationTests;

/// <summary>
/// Exercises the "What now?" flow through real Postgres: energy gating, surfacing,
/// snooze, and complete all round-trip through EF Core.
/// </summary>
public class WhatNowTests : IAsyncLifetime
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

    private async Task Seed()
    {
        var repo = new TaskRepository(_factory);
        await repo.AddAsync(new TaskItem("high scary", EnergyCost.High, Importance.High));
        await repo.AddAsync(new TaskItem("easy win", EnergyCost.Low, Importance.Low));
        await repo.SaveChangesAsync();
    }

    [Fact]
    public async Task WhatNow_WhenFried_SurfacesOnlyLowCost()
    {
        await Seed();
        var handler = new GetWhatNowHandler(new TaskRepository(_factory));

        var result = await handler.Handle(new GetWhatNowQuery(EnergyLevel.Fried), CancellationToken.None);

        Assert.NotNull(result.Now);
        Assert.Equal("easy win", result.Now!.Task.Title);
        Assert.Equal(1, result.EligibleCount);
    }

    [Fact]
    public async Task Complete_RemovesTaskFromWhatNow()
    {
        await Seed();
        var repo = new TaskRepository(_factory);
        var whatNow = new GetWhatNowHandler(repo);
        var complete = new CompleteTaskHandler(repo);

        var before = await whatNow.Handle(new GetWhatNowQuery(EnergyLevel.Good), CancellationToken.None);
        var topId = before.Now!.Task.Id;

        await complete.Handle(new CompleteTaskCommand(topId), CancellationToken.None);

        var after = await whatNow.Handle(new GetWhatNowQuery(EnergyLevel.Good), CancellationToken.None);
        Assert.NotEqual(topId, after.Now?.Task.Id);
    }

    [Fact]
    public async Task Snooze_IncrementsCount_AndPersists()
    {
        await Seed();
        var repo = new TaskRepository(_factory);
        var snooze = new SnoozeTaskHandler(repo);
        var all = await repo.GetOpenAsync();
        var target = all.First();

        await snooze.Handle(new SnoozeTaskCommand(target.Id), CancellationToken.None);

        var reloaded = await new TaskRepository(_factory).GetByIdAsync(target.Id);
        Assert.Equal(1, reloaded!.SnoozeCount);
        Assert.NotNull(reloaded.LastSnoozedAt);
    }
}
