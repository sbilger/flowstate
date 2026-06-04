using FlowState.Application.Common;
using FlowState.Domain.Scoring;
using MediatR;

namespace FlowState.Application.Tasks;

/// <summary>
/// Runs the decay/resurfacing lifecycle over all open tasks. Invoked on a schedule by the
/// background worker (Hangfire). Returns counts for logging.
/// </summary>
public record RunDecaySweepCommand : IRequest<DecaySweepResult>;

public class RunDecaySweepHandler : IRequestHandler<RunDecaySweepCommand, DecaySweepResult>
{
    private readonly ITaskRepository _repository;

    public RunDecaySweepHandler(ITaskRepository repository) => _repository = repository;

    public async Task<DecaySweepResult> Handle(RunDecaySweepCommand request, CancellationToken cancellationToken)
    {
        var openTracked = await _repository.GetOpenTrackedAsync(cancellationToken);
        var result = DecayProcessor.Process(openTracked, DateTimeOffset.UtcNow);

        if (result.WentDormant > 0 || result.Resurfaced > 0)
            await _repository.SaveChangesAsync(cancellationToken);

        return result;
    }
}
