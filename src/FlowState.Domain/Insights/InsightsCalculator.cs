using FlowState.Domain.Focus;
using FlowState.Domain.Tasks;

namespace FlowState.Domain.Insights;

/// <summary>
/// Pure time-series aggregation over tasks + focus sessions. No DB, no framework — the
/// repository loads the rows, this turns them into the Insights payload. Also exposes the
/// "peak window" used to feed learned patterns back into the scoring engine.
/// </summary>
public static class InsightsCalculator
{
    public static InsightsResult Compute(
        IReadOnlyList<TaskItem> allTasks,
        IReadOnlyList<FocusSession> sessions,
        DateTimeOffset now,
        TimeSpan? localOffset = null)
    {
        var offset = localOffset ?? TimeSpan.Zero;

        var completed = allTasks.Where(t => t.Status == TaskItemStatus.Done && t.CompletedAt is not null).ToList();
        var endedSessions = sessions.Where(s => s.EndedAt is not null).ToList();
        var completedSessions = endedSessions.Where(s => s.Outcome == FocusOutcome.Completed).ToList();

        // Completions by local hour (drives peak-window chart).
        var byHour = Enumerable.Range(0, 24).Select(h => new HourBucket(h, 0)).ToArray();
        foreach (var t in completed)
        {
            int hour = t.CompletedAt!.Value.ToOffset(offset).Hour;
            byHour[hour] = byHour[hour] with { Count = byHour[hour].Count + 1 };
        }

        // Average focus minutes (over ended sessions).
        int avgMinutes = endedSessions.Count == 0
            ? 0
            : (int)Math.Round(endedSessions.Average(s => s.Elapsed(now).TotalMinutes));

        // Completion rate = completed / ended sessions.
        double completionRate = endedSessions.Count == 0
            ? 0
            : completedSessions.Count / (double)endedSessions.Count;

        // Energy-vs-done.
        var energyBreakdown = new[] { EnergyCost.Low, EnergyCost.Medium, EnergyCost.High }
            .Select(e => new EnergyBreakdown(e, completed.Count(t => t.EnergyCost == e)))
            .ToList();

        // Most avoided (open, snoozed).
        var avoided = allTasks
            .Where(t => t.Status == TaskItemStatus.Open && t.SnoozeCount > 0)
            .OrderByDescending(t => t.SnoozeCount)
            .ThenByDescending(t => (now - t.CreatedAt).TotalDays)
            .Take(5)
            .Select(t => new AvoidedTask(t.Title, t.SnoozeCount, Math.Round((now - t.CreatedAt).TotalDays, 1)))
            .ToList();

        var peak = PeakWindowStart(byHour);

        return new InsightsResult(
            completed.Count,
            endedSessions.Count,
            completedSessions.Count,
            completionRate,
            avgMinutes,
            byHour,
            peak,
            energyBreakdown,
            avoided);
    }

    /// <summary>
    /// The start hour of the best contiguous 3-hour window by completion count, or null if
    /// there's no completion data. Used both for display and to seed the scoring engine's
    /// time-of-day fit (closing the learning loop).
    /// </summary>
    public static int? PeakWindowStart(IReadOnlyList<HourBucket> byHour)
    {
        if (byHour.Count < 3 || byHour.All(b => b.Count == 0)) return null;

        int bestStart = 0, bestSum = -1;
        for (int start = 0; start <= 21; start++)
        {
            int sum = byHour[start].Count + byHour[start + 1].Count + byHour[start + 2].Count;
            if (sum > bestSum)
            {
                bestSum = sum;
                bestStart = start;
            }
        }
        return bestSum > 0 ? bestStart : null;
    }
}
