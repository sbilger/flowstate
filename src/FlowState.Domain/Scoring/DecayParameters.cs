namespace FlowState.Domain.Scoring;

/// <summary>
/// Tunable parameters for the decay/resurfacing lifecycle. Kept as a record so it's
/// trivially configurable and unit-testable. Defaults are the starting values from
/// docs/research/decay-resurfacing.md §4.5.
/// </summary>
public record DecayParameters
{
    /// <summary>Days for an untouched task's freshness to halve (exponential decay).</summary>
    public double FreshnessHalfLifeDays { get; init; } = 5.0;

    /// <summary>Freshness below this (or enough snoozes) makes a task eligible to go Dormant.</summary>
    public double DormancyFreshnessThreshold { get; init; } = 0.25;

    /// <summary>Snooze count that alone makes a task eligible to go Dormant.</summary>
    public int DormancySnoozeThreshold { get; init; } = 3;

    /// <summary>How fast aging pressure rebuilds per day of dormancy (linear).</summary>
    public double AgingRatePerDay { get; init; } = 0.10;

    /// <summary>Aging pressure (× importance) at/above which a Dormant task is force-resurfaced.</summary>
    public double ResurfaceThreshold { get; init; } = 1.0;

    /// <summary>Absolute anti-starvation guarantee: resurface after this many dormant days regardless.</summary>
    public double HardResurfaceCeilingDays { get; init; } = 14.0;

    public static DecayParameters Default => new();

    /// <summary>Decay constant λ derived from the half-life: freshness = exp(−λ·days).</summary>
    public double Lambda => Math.Log(2) / FreshnessHalfLifeDays;
}
