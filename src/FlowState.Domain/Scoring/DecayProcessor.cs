using FlowState.Domain.Tasks;

namespace FlowState.Domain.Scoring;

/// <summary>Outcome of a decay sweep, for logging/telemetry.</summary>
public record DecaySweepResult(int WentDormant, int Resurfaced);

/// <summary>
/// Applies the decay/resurfacing lifecycle transitions to a batch of tasks. Pure domain
/// logic: the background worker loads open tasks, calls this, then persists. Keeping the
/// transition logic here (not in the worker) keeps it unit-testable without infrastructure.
/// </summary>
public static class DecayProcessor
{
    public static DecaySweepResult Process(IEnumerable<TaskItem> openTasks, DateTimeOffset now, DecayParameters? p = null)
    {
        var pars = p ?? DecayParameters.Default;
        int wentDormant = 0, resurfaced = 0;

        foreach (var task in openTasks)
        {
            // Active → Dormant
            if (task.DecayState == DecayState.Active && DecayEngine.ShouldGoDormant(task, now, pars))
            {
                task.GoDormant(now);
                wentDormant++;
                continue;
            }

            // Dormant → Resurfaced (aging / anti-starvation)
            if (task.DecayState == DecayState.Dormant && DecayEngine.ShouldResurface(task, now, pars))
            {
                task.Resurface();
                resurfaced++;
            }
        }

        return new DecaySweepResult(wentDormant, resurfaced);
    }
}
