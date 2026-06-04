using FlowState.Application.Tasks;
using FlowState.Domain.Tasks;
using FlowState.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace FlowState.IntegrationTests;

/// <summary>
/// Spins up a real Postgres in Docker, migrates, seeds, and verifies the
/// task round-trip through EF Core + the repository.
/// </summary>
public class GetTasksTests : IAsyncLifetime
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
    public async Task AddedTasks_AreReturned_NewestFirst()
    {
        var repo = new TaskRepository(_factory);
        await repo.AddAsync(new TaskItem("first", EnergyCost.Low));
        await repo.SaveChangesAsync();
        await Task.Delay(10);
        await repo.AddAsync(new TaskItem("second", EnergyCost.High));
        await repo.SaveChangesAsync();

        var handler = new GetTasksHandler(repo);
        var result = await handler.Handle(new GetTasksQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("second", result[0].Title); // newest first
        Assert.Equal("first", result[1].Title);
    }
}
