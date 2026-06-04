namespace FlowState.Domain.Scoring;

/// <summary>
/// Tunable weights for the priority score. Lives as a record so it's trivially
/// configurable and unit-testable. Defaults are the v1 starting point from ALGORITHM.md.
/// </summary>
public record ScoringWeights
{
    public double Importance { get; init; } = 0.35;
    public double Age { get; init; } = 0.20;
    public double TimeOfDayFit { get; init; } = 0.20;
    public double SnoozePenalty { get; init; } = 0.15;

    /// <summary>Age (in days) at which the age factor saturates — a task can't climb forever.</summary>
    public double AgeSaturationDays { get; init; } = 14.0;

    /// <summary>Snooze count at which the snooze penalty saturates.</summary>
    public int SnoozeSaturationCount { get; init; } = 6;

    /// <summary>Multiplier applied when a task's energy cost exceeds the user's current energy.</summary>
    public double OverBudgetGate { get; init; } = 0.05;

    /// <summary>Small boost when a task's energy cost exactly matches the user's current energy.</summary>
    public double ExactEnergyMatchBoost { get; init; } = 1.10;

    public static ScoringWeights Default => new();
}
