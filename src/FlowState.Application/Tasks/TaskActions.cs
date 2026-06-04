using FlowState.Application.Common;
using MediatR;

namespace FlowState.Application.Tasks;

/// <summary>Mark a task done (the "✓ Done" action).</summary>
public record CompleteTaskCommand(Guid Id) : IRequest<bool>;

public class CompleteTaskHandler : IRequestHandler<CompleteTaskCommand, bool>
{
    private readonly ITaskRepository _repository;
    public CompleteTaskHandler(ITaskRepository repository) => _repository = repository;

    public async Task<bool> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (task is null) return false;
        task.Complete();
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}

/// <summary>Dismiss a task from "What now?" (the "Not now" action). Records avoidance for decay.</summary>
public record SnoozeTaskCommand(Guid Id) : IRequest<bool>;

public class SnoozeTaskHandler : IRequestHandler<SnoozeTaskCommand, bool>
{
    private readonly ITaskRepository _repository;
    public SnoozeTaskHandler(ITaskRepository repository) => _repository = repository;

    public async Task<bool> Handle(SnoozeTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (task is null) return false;
        task.Snooze();
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}

/// <summary>Delete a task outright.</summary>
public record DeleteTaskCommand(Guid Id) : IRequest<bool>;

public class DeleteTaskHandler : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly ITaskRepository _repository;
    public DeleteTaskHandler(ITaskRepository repository) => _repository = repository;

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (task is null) return false;
        _repository.Remove(task);
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}

/// <summary>
/// Resurfacing decision: "keep it" / "do it" — wakes a resurfaced or dormant task back to
/// Active, refreshing its decay clock so it re-enters "What now?".
/// </summary>
public record KeepTaskCommand(Guid Id) : IRequest<bool>;

public class KeepTaskHandler : IRequestHandler<KeepTaskCommand, bool>
{
    private readonly ITaskRepository _repository;
    public KeepTaskHandler(ITaskRepository repository) => _repository = repository;

    public async Task<bool> Handle(KeepTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (task is null) return false;
        task.Touch(); // refreshes freshness + returns to Active
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
