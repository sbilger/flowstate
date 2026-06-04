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
    public static double Score(TaskItem task, EnergyLevel currentEnergy, DateTimeOffset now,
        ScoringWeights? weights = null, DecayParameters? decay = null, int? learnedPeakHourStart = null)
    {
        var w = weights ?? ScoringWeights.Default;

        // --- normalized factors (each 0..1) ---
        double importanceNorm = ((int)task.Importance - 1) / 2.0;           // Low=0, Normal=.5, High=1
        double ageDays = Math.Max(0, (now - task.CreatedAt).TotalDays);
        double ageNorm = Math.Clamp(ageDays / w.AgeSaturationDays, 0, 1);
        double timeFit = TimeOfDayFit(task.EnergyCost, now, learnedPeakHourStart);                // 0..1
        double snoozeNorm = Math.Clamp(task.SnoozeCount / (double)w.SnoozeSaturationCount, 0, 1);

        double baseScore =
              w.Importance * importanceNorm
            + w.Age * ageNorm
            + w.TimeOfDayFit * timeFit
            - w.SnoozePenalty * snoozeNorm;

        baseScore = Math.Max(0, baseScore);

        // --- energy gate (hard multiplier) ---
        double gate = EnergyGate(task.EnergyCost, currentEnergy, w);

        // --- freshness decay multiplier (opt-in; defaults to no decay when null) ---
        double freshness = decay is null ? 1.0 : DecayEngine.Freshness(task, now, decay);

        return baseScore * gate * freshness;
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
    /// Time-of-day fit (0..1). Falls back to a static heuristic (high-cost work fits mornings,
    /// medium midday, low evenings). When a LEARNED peak window is supplied (from Insights),
    /// being inside it boosts higher-cost work — the app adapting to when you actually deliver.
    /// </summary>
    public static double TimeOfDayFit(EnergyCost cost, DateTimeOffset now, int? learnedPeakHourStart = null)
    {
        int hour = now.Hour;

        if (learnedPeakHourStart is int peak)
        {
            bool inPeak = IsWithinWindow(hour, peak, 3);
            return cost switch
            {
                EnergyCost.High => inPeak ? 1.0 : 0.4,
                EnergyCost.Medium => inPeak ? 0.85 : 0.55,
                EnergyCost.Low => inPeak ? 0.6 : 0.7,   // save easy wins for off-peak
                _ => 0.5
            };
        }

        // Static fallback until there's enough data to learn a peak.
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

    private static bool IsWithinWindow(int hour, int start, int length)
    {
        for (int i = 0; i < length; i++)
            if ((start + i) % 24 == hour) return true;
        return false;
    }
}
