using FlowState.Domain.Tasks;

namespace FlowState.Domain.Scoring;

/// <summary>
/// The algorithmic heart of FlowState. Pure and deterministic: given a task and the
/// user's current context (energy + time), produce a PriorityScore used to rank tasks
/// for "What now?". The energy gate is a HARD multiplier so the app never surfaces work
/// the user can't take on — encoding the product promise, not just math.
/// </summary>
public static class TaskScorer
{
    /// <summary>
    /// Score a task in 0..~1 (the energy boost can nudge slightly above 1). Higher = surface sooner.
    /// </summary>
    public static double Score(TaskItem task, EnergyLevel currentEnergy, DateTimeOffset now, ScoringWeights? weights = null)
    {
        var w = weights ?? ScoringWeights.Default;

        // --- normalized factors (each 0..1) ---
        double importanceNorm = ((int)task.Importance - 1) / 2.0;           // Low=0, Normal=.5, High=1
        double ageDays = Math.Max(0, (now - task.CreatedAt).TotalDays);
        double ageNorm = Math.Clamp(ageDays / w.AgeSaturationDays, 0, 1);
        double timeFit = TimeOfDayFit(task.EnergyCost, now);                // 0..1
        double snoozeNorm = Math.Clamp(task.SnoozeCount / (double)w.SnoozeSaturationCount, 0, 1);

        double baseScore =
              w.Importance * importanceNorm
            + w.Age * ageNorm
            + w.TimeOfDayFit * timeFit
            - w.SnoozePenalty * snoozeNorm;

        baseScore = Math.Max(0, baseScore);

        // --- energy gate (hard multiplier) ---
        double gate = EnergyGate(task.EnergyCost, currentEnergy, w);

        return baseScore * gate;
    }

    /// <summary>
    /// Hard gate: tasks costing more energy than the user currently has are pushed far down
    /// (multiplied by OverBudgetGate). An exact match gets a small boost. Within-budget but
    /// not-exact tasks pass through unchanged.
    /// </summary>
    public static double EnergyGate(EnergyCost cost, EnergyLevel energy, ScoringWeights w)
    {
        int costLevel = (int)cost;     // Low=1, Med=2, High=3
        int energyLevel = (int)energy; // Fried=1, Okay=2, Good=3

        if (costLevel > energyLevel) return w.OverBudgetGate;       // over budget — gated out
        if (costLevel == energyLevel) return w.ExactEnergyMatchBoost; // perfect fit — boosted
        return 1.0;                                                 // comfortably within budget
    }

    /// <summary>True when the task is within the user's current energy budget (passes the gate).</summary>
    public static bool PassesEnergyGate(EnergyCost cost, EnergyLevel energy) => (int)cost <= (int)energy;

    /// <summary>
    /// A simple, deterministic time-of-day heuristic until Slice 5 learns real patterns:
    /// high-cost work fits mornings, medium fits midday, low fits evenings.
    /// Returns 0..1.
    /// </summary>
    public static double TimeOfDayFit(EnergyCost cost, DateTimeOffset now)
    {
        int hour = now.ToLocalTime().Hour;
        // 0 = night, simple buckets
        bool morning = hour >= 6 && hour < 12;
        bool midday = hour >= 12 && hour < 17;
        bool evening = hour >= 17 && hour < 23;

        return cost switch
        {
            EnergyCost.High => morning ? 1.0 : midday ? 0.5 : 0.2,
            EnergyCost.Medium => midday ? 1.0 : morning ? 0.6 : 0.4,
            EnergyCost.Low => evening ? 1.0 : 0.6,
            _ => 0.5
        };
    }
}
