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

        // Blazor Server caveat: a DI scope lives for the whole circuit, so a single scoped
        // DbContext gets shared across components that query concurrently (e.g. the home page
        // and the resurfaced banner both initializing) -> "second operation started on this
        // context". The factory hands each unit of work its own short-lived context.
        services.AddDbContextFactory<FlowStateDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Transient so each handler/component gets a fresh repository (and thus its own context).
        services.AddTransient<ITaskRepository, TaskRepository>();
        services.AddTransient<IFocusSessionRepository, FocusSessionRepository>();

        return services;
    }
}
