using FlowState.Domain.Tasks;

namespace FlowState.Domain.Insights;

/// <summary>One hour-of-day bucket with a count (for the peak-window chart).</summary>
public record HourBucket(int Hour, int Count);

/// <summary>Completion count per energy cost (energy-vs-done chart).</summary>
public record EnergyBreakdown(EnergyCost EnergyCost, int Completed);

/// <summary>A task that's been avoided, for the "most put off" list.</summary>
public record AvoidedTask(string Title, int SnoozeCount, double DaysOld);

/// <summary>The full insights payload.</summary>
public record InsightsResult(
    int CompletedCount,
    int FocusSessionCount,
    int CompletedFocusCount,
    double CompletionRate,            // completed sessions / total sessions (0..1)
    int AverageFocusMinutes,
    IReadOnlyList<HourBucket> CompletionsByHour,
    int? PeakHourStart,               // start of the best 3-hour window, or null
    IReadOnlyList<EnergyBreakdown> EnergyBreakdown,
    IReadOnlyList<AvoidedTask> MostAvoided)
{
    public bool HasData => CompletedCount > 0 || FocusSessionCount > 0;

    public static InsightsResult Empty => new(
        0, 0, 0, 0, 0,
        Array.Empty<HourBucket>(), null,
        Array.Empty<EnergyBreakdown>(), Array.Empty<AvoidedTask>());
}
