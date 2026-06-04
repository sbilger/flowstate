using FlowState.Application.Common;
using FlowState.Domain.Insights;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace FlowState.Application.Insights;

/// <summary>Computes the Insights payload. Cached briefly since it scans all tasks + sessions.</summary>
public record GetInsightsQuery(int LocalOffsetHours = 0) : IRequest<InsightsResult>;

public class GetInsightsHandler : IRequestHandler<GetInsightsQuery, InsightsResult>
{
    private readonly ITaskRepository _tasks;
    private readonly IFocusSessionRepository _sessions;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan CacheFor = TimeSpan.FromSeconds(30);

    public GetInsightsHandler(ITaskRepository tasks, IFocusSessionRepository sessions, IMemoryCache cache)
    {
        _tasks = tasks;
        _sessions = sessions;
        _cache = cache;
    }

    public async Task<InsightsResult> Handle(GetInsightsQuery request, CancellationToken cancellationToken)
    {
        var key = $"insights:{request.LocalOffsetHours}";
        if (_cache.TryGetValue(key, out InsightsResult? cached) && cached is not null)
            return cached;

        var allTasks = await _tasks.GetAllAsync(cancellationToken);
        var sessions = await _sessions.GetAllAsync(cancellationToken);

        var result = InsightsCalculator.Compute(
            allTasks, sessions, DateTimeOffset.UtcNow, TimeSpan.FromHours(request.LocalOffsetHours));

        _cache.Set(key, result, CacheFor);
        return result;
    }
}
