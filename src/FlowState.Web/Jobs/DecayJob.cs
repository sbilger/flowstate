using FlowState.Application.Tasks;
using MediatR;

namespace FlowState.Web.Jobs;

/// <summary>
/// Hangfire recurring job: runs the decay/resurfacing sweep. Registered to run on a
/// schedule; logs how many tasks went dormant or were resurfaced.
/// </summary>
public class DecayJob
{
    private readonly IMediator _mediator;
    private readonly ILogger<DecayJob> _logger;

    public DecayJob(IMediator mediator, ILogger<DecayJob> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        var result = await _mediator.Send(new RunDecaySweepCommand());
        if (result.WentDormant > 0 || result.Resurfaced > 0)
        {
            _logger.LogInformation(
                "Decay sweep: {Dormant} went dormant, {Resurfaced} resurfaced.",
                result.WentDormant, result.Resurfaced);
        }
    }
}
