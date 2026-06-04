using FlowState.Application.Common;
using FlowState.Application.Insights;
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
    private readonly IMediator? _mediator;

    public GetWhatNowHandler(ITaskRepository repository, IMediator? mediator = null)
    {
        _repository = repository;
        _mediator = mediator;
    }

    public async Task<WhatNowResult> Handle(GetWhatNowQuery request, CancellationToken cancellationToken)
    {
        var open = await _repository.GetOpenAsync(cancellationToken);

        // Closing the learning loop: feed the user's learned peak window into scoring.
        int? peak = _mediator is null
            ? null
            : (await _mediator.Send(new GetInsightsQuery(), cancellationToken)).PeakHourStart;

        var ranked = WhatNowSelector.Rank(
            open, request.Energy, DateTimeOffset.Now,
            learnedPeakHourStart: peak);

        if (ranked.Count == 0)
            return new WhatNowResult(null, Array.Empty<TaskDto>(), 0);

        var top = ranked[0];
        var surfaced = new SurfacedTaskDto(
            TaskDto.From(top.Task),
            top.Score,
            ReasonFor(top.Task, request.Energy, peak));

        var nextUp = ranked.Skip(1).Take(3).Select(s => TaskDto.From(s.Task)).ToList();

        return new WhatNowResult(surfaced, nextUp, ranked.Count);
    }

    /// <summary>A short, human explanation of why this task surfaced — shown under the hero card.</summary>
    private static string ReasonFor(TaskItem task, EnergyLevel energy, int? peak)
    {
        if (task.Importance == Importance.High) return "high priority right now";
        if (task.SnoozeCount >= 3) return "you've put this off a few times";
        if (peak is int p && DateTimeOffset.Now.Hour >= p && DateTimeOffset.Now.Hour < p + 3
            && task.EnergyCost == EnergyCost.High)
            return "this is your peak focus window";
        if ((int)task.EnergyCost == (int)energy) return "a good fit for your energy";
        var ageDays = (DateTimeOffset.UtcNow - task.CreatedAt).TotalDays;
        if (ageDays >= 3) return "this has been waiting a while";
        return "a solid pick for right now";
    }
}
