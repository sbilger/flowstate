using FlowState.Domain.Tasks;

namespace FlowState.Domain.Scoring;

/// <summary>
/// The decay/resurfacing engine. Pure and deterministic. Two dynamics:
///   1. Freshness decay  — untouched tasks lose salience (exponential, tunable half-life).
///   2. Aging / anti-starvation — borrowed from OS schedulers, a Dormant task's pressure
///      climbs with neglect until it's force-resurfaced as a decision. Nothing rots silently.
/// </summary>
public static class DecayEngine
{
    /// <summary>
    /// Freshness in 0..1: 1.0 just-touched, decaying exponentially toward 0 with neglect.
    /// </summary>
    public static double Freshness(TaskItem task, DateTimeOffset now, DecayParameters? p = null)
    {
        var pars = p ?? DecayParameters.Default;
        double days = Math.Max(0, (now - task.LastTouchedAt).TotalDays);
        return Math.Exp(-pars.Lambda * days);
    }

    /// <summary>
    /// Should an Active task go Dormant? True when it has decayed past the freshness
    /// threshold OR been snoozed enough to count as actively avoided.
    /// </summary>
    public static bool ShouldGoDormant(TaskItem task, DateTimeOffset now, DecayParameters? p = null)
    {
        if (task.Status != TaskItemStatus.Open) return false;
        if (task.DecayState != DecayState.Active) return false;

        var pars = p ?? DecayParameters.Default;
        bool decayedOut = Freshness(task, now, pars) < pars.DormancyFreshnessThreshold;
        bool avoided = task.SnoozeCount >= pars.DormancySnoozeThreshold;
        return decayedOut || avoided;
    }

    /// <summary>
    /// Aging pressure for a Dormant task: grows linearly with days-dormant, scaled by importance,
    /// so important neglected work surfaces sooner. 0 for non-dormant tasks.
    /// </summary>
    public static double AgingPressure(TaskItem task, DateTimeOffset now, DecayParameters? p = null)
    {
        if (task.DecayState != DecayState.Dormant || task.DormantSince is null) return 0;

        var pars = p ?? DecayParameters.Default;
        double dormantDays = Math.Max(0, (now - task.DormantSince.Value).TotalDays);
        double importanceScale = 0.5 + ((int)task.Importance - 1) / 2.0; // Low=.5, Normal=1.0, High=1.5
        return dormantDays * pars.AgingRatePerDay * importanceScale;
    }

    /// <summary>
    /// Should a Dormant task be force-resurfaced? True when aging pressure crosses the
    /// threshold OR the hard anti-starvation ceiling is hit — the guarantee nothing starves.
    /// </summary>
    public static bool ShouldResurface(TaskItem task, DateTimeOffset now, DecayParameters? p = null)
    {
        if (task.Status != TaskItemStatus.Open) return false;
        if (task.DecayState != DecayState.Dormant || task.DormantSince is null) return false;

        var pars = p ?? DecayParameters.Default;
        double dormantDays = Math.Max(0, (now - task.DormantSince.Value).TotalDays);

        if (dormantDays >= pars.HardResurfaceCeilingDays) return true;          // hard guarantee
        return AgingPressure(task, now, pars) >= pars.ResurfaceThreshold;       // pressure-driven
    }
}
