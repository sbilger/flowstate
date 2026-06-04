using FlowState.Application.Common;
using FlowState.Domain.Tasks;
using MediatR;

namespace FlowState.Application.Tasks;

/// <summary>Captures a new task (the "brain dump" action).</summary>
public record AddTaskCommand(string Title, EnergyCost EnergyCost, Importance Importance = Importance.Normal)
    : IRequest<Guid>;

public class AddTaskHandler : IRequestHandler<AddTaskCommand, Guid>
{
    private readonly ITaskRepository _repository;

    public AddTaskHandler(ITaskRepository repository) => _repository = repository;

    public async Task<Guid> Handle(AddTaskCommand request, CancellationToken cancellationToken)
    {
        var task = new TaskItem(request.Title, request.EnergyCost, request.Importance);
        await _repository.AddAsync(task, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return task.Id;
    }
}
