using FlowState.Application.Common;
using FlowState.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowState.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FlowState")
            ?? throw new InvalidOperationException("Connection string 'FlowState' not configured.");

        services.AddDbContext<FlowStateDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }
}
