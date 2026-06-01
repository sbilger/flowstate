using FlowState.Application.Common;
using FlowState.Domain.Tasks;
using MediatR;

namespace FlowState.Application.Tasks;

/// <summary>A task as exposed to the UI/API.</summary>
public record TaskDto(Guid Id, string Title, EnergyCost EnergyCost, TaskItemStatus Status, DateTimeOffset CreatedAt);

/// <summary>Returns all tasks. Slice 1 walking-skeleton query.</summary>
public record GetTasksQuery : IRequest<IReadOnlyList<TaskDto>>;

public class GetTasksHandler : IRequestHandler<GetTasksQuery, IReadOnlyList<TaskDto>>
{
    private readonly ITaskRepository _repository;

    public GetTasksHandler(ITaskRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _repository.GetAllAsync(cancellationToken);
        return tasks
            .Select(t => new TaskDto(t.Id, t.Title, t.EnergyCost, t.Status, t.CreatedAt))
            .ToList();
    }
}
