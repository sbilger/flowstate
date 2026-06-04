using FlowState.Application.Common;
using FlowState.Domain.Tasks;
using MediatR;

namespace FlowState.Application.Tasks;

/// <summary>A resurfaced task plus how long it had been dormant, for the decision prompt.</summary>
public record ResurfacedTaskDto(TaskDto Task, int DaysWaited);

/// <summary>Tasks the resurfacing worker has flagged for a keep/reschedule/break-down/drop decision.</summary>
public record GetResurfacedQuery : IRequest<IReadOnlyList<ResurfacedTaskDto>>;

public class GetResurfacedHandler : IRequestHandler<GetResurfacedQuery, IReadOnlyList<ResurfacedTaskDto>>
{
    private readonly ITaskRepository _repository;

    public GetResurfacedHandler(ITaskRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<ResurfacedTaskDto>> Handle(GetResurfacedQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _repository.GetResurfacedAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        return tasks
            .Select(t =>
            {
                var since = t.DormantSince ?? t.LastTouchedAt;
                var days = Math.Max(1, (int)Math.Round((now - since).TotalDays));
                return new ResurfacedTaskDto(TaskDto.From(t), days);
            })
            .ToList();
    }
}
