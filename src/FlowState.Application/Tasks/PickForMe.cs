using FlowState.Application.Common;
using FlowState.Domain.Scoring;
using FlowState.Domain.Tasks;
using MediatR;

namespace FlowState.Application.Tasks;

/// <summary>Dopamine-friendly weighted-random pick among energy-eligible open tasks.</summary>
public record PickForMeQuery(EnergyLevel Energy) : IRequest<SurfacedTaskDto?>;

public class PickForMeHandler : IRequestHandler<PickForMeQuery, SurfacedTaskDto?>
{
    private readonly ITaskRepository _repository;

    public PickForMeHandler(ITaskRepository repository) => _repository = repository;

    public async Task<SurfacedTaskDto?> Handle(PickForMeQuery request, CancellationToken cancellationToken)
    {
        var open = await _repository.GetOpenAsync(cancellationToken);
        var pick = WhatNowSelector.PickForMe(open, request.Energy, DateTimeOffset.UtcNow, Random.Shared);
        if (pick is null) return null;

        return new SurfacedTaskDto(TaskDto.From(pick.Task), pick.Score, "rolled for you");
    }
}
