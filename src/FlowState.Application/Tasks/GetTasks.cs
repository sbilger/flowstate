using FlowState.Application.Common;
using MediatR;

namespace FlowState.Application.Tasks;

/// <summary>Returns all tasks (newest first).</summary>
public record GetTasksQuery(bool OpenOnly = false) : IRequest<IReadOnlyList<TaskDto>>;

public class GetTasksHandler : IRequestHandler<GetTasksQuery, IReadOnlyList<TaskDto>>
{
    private readonly ITaskRepository _repository;

    public GetTasksHandler(ITaskRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = request.OpenOnly
            ? await _repository.GetOpenAsync(cancellationToken)
            : await _repository.GetAllAsync(cancellationToken);

        return tasks.Select(TaskDto.From).ToList();
    }
}
