using FlowState.Application.Common;
using FlowState.Domain.Scoring;
using FlowState.Domain.Tasks;
using MediatR;

namespace FlowState.Application.Tasks;

/// <summary>The "What now?" payload: the surfaced task plus the next few, for the hero screen.</summary>
public record WhatNowResult(SurfacedTaskDto? Now, IReadOnlyList<TaskDto> NextUp, int EligibleCount);

/// <summary>Surface the single best task for the user's current energy + the next few.</summary>
public record GetWhatNowQuery(EnergyLevel Energy) : IRequest<WhatNowResult>;

public class GetWhatNowHandler : IRequestHandler<GetWhatNowQuery, WhatNowResult>
{
    private readonly ITaskRepository _repository;

    public GetWhatNowHandler(ITaskRepository repository) => _repository = repository;

    public async Task<WhatNowResult> Handle(GetWhatNowQuery request, CancellationToken cancellationToken)
    {
        var open = await _repository.GetOpenAsync(cancellationToken);
        var ranked = WhatNowSelector.Rank(open, request.Energy, DateTimeOffset.UtcNow);

        if (ranked.Count == 0)
            return new WhatNowResult(null, Array.Empty<TaskDto>(), 0);

        var top = ranked[0];
        var surfaced = new SurfacedTaskDto(
            TaskDto.From(top.Task),
            top.Score,
            ReasonFor(top.Task, request.Energy));

        var nextUp = ranked.Skip(1).Take(3).Select(s => TaskDto.From(s.Task)).ToList();

        return new WhatNowResult(surfaced, nextUp, ranked.Count);
    }

    /// <summary>A short, human explanation of why this task surfaced — shown under the hero card.</summary>
    private static string ReasonFor(TaskItem task, EnergyLevel energy)
    {
        if (task.Importance == Importance.High) return "high priority right now";
        if (task.SnoozeCount >= 3) return "you've put this off a few times";
        if ((int)task.EnergyCost == (int)energy) return "a good fit for your energy";
        var ageDays = (DateTimeOffset.UtcNow - task.CreatedAt).TotalDays;
        if (ageDays >= 3) return "this has been waiting a while";
        return "a solid pick for right now";
    }
}
