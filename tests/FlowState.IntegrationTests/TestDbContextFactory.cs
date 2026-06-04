using FlowState.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowState.IntegrationTests;

/// <summary>
/// Minimal IDbContextFactory for tests: hands out contexts bound to the test container's
/// connection string, mirroring how the app's repositories obtain their own context.
/// </summary>
public class TestDbContextFactory : IDbContextFactory<FlowStateDbContext>
{
    private readonly string _connectionString;

    public TestDbContextFactory(string connectionString) => _connectionString = connectionString;

    public FlowStateDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FlowStateDbContext>()
            .UseNpgsql(_connectionString)
            .Options;
        return new FlowStateDbContext(options);
    }
}
