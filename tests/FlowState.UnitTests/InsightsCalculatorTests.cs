using FlowState.Domain.Focus;
using FlowState.Domain.Insights;
using FlowState.Domain.Tasks;

namespace FlowState.UnitTests;

public class InsightsCalculatorTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Empty_WhenNoData()
    {
        var result = InsightsCalculator.Compute(
            Array.Empty<TaskItem>(), Array.Empty<FocusSession>(), Now);

        Assert.False(result.HasData);
        Assert.Equal(0, result.CompletedCount);
        Assert.Null(result.PeakHourStart);
    }

    [Fact]
    public void CountsCompletedTasks()
    {
        var t1 = new TaskItem("a", EnergyCost.Low); t1.Complete();
        var t2 = new TaskItem("b", EnergyCost.High); t2.Complete();
        var t3 = new TaskItem("c", EnergyCost.Medium); // still open

        var result = InsightsCalculator.Compute(new[] { t1, t2, t3 }, Array.Empty<FocusSession>(), Now);

        Assert.Equal(2, result.CompletedCount);
        Assert.True(result.HasData);
    }

    [Fact]
    public void EnergyBreakdown_CountsByCost()
    {
        var low = new TaskItem("l", EnergyCost.Low); low.Complete();
        var high1 = new TaskItem("h1", EnergyCost.High); high1.Complete();
        var high2 = new TaskItem("h2", EnergyCost.High); high2.Complete();

        var result = InsightsCalculator.Compute(new[] { low, high1, high2 }, Array.Empty<FocusSession>(), Now);

        Assert.Equal(1, result.EnergyBreakdown.Single(e => e.EnergyCost == EnergyCost.Low).Completed);
        Assert.Equal(2, result.EnergyBreakdown.Single(e => e.EnergyCost == EnergyCost.High).Completed);
    }

    [Fact]
    public void MostAvoided_RanksBySnoozeCount()
    {
        var a = new TaskItem("rarely", EnergyCost.Low); a.Snooze();
        var b = new TaskItem("often", EnergyCost.Low); b.Snooze(); b.Snooze(); b.Snooze();

        var result = InsightsCalculator.Compute(new[] { a, b }, Array.Empty<FocusSession>(), Now);

        Assert.Equal("often", result.MostAvoided[0].Title);
        Assert.Equal(3, result.MostAvoided[0].SnoozeCount);
    }

    [Fact]
    public void CompletionRate_FromSessions()
    {
        var done = new FocusSession(Guid.NewGuid(), "x", TimeSpan.FromMinutes(15));
        done.End(FocusOutcome.Completed);
        var bailed = new FocusSession(Guid.NewGuid(), "y", TimeSpan.FromMinutes(15));
        bailed.End(FocusOutcome.Abandoned);

        var result = InsightsCalculator.Compute(Array.Empty<TaskItem>(), new[] { done, bailed }, Now);

        Assert.Equal(2, result.FocusSessionCount);
        Assert.Equal(0.5, result.CompletionRate, 3);
    }

    [Fact]
    public void PeakWindow_FindsBestThreeHourBlock()
    {
        // Hand-build 24 buckets with a clear cluster at 9-11.
        var buckets = Enumerable.Range(0, 24)
            .Select(h => new HourBucket(h, h is 9 or 10 or 11 ? 5 : 0))
            .ToList();

        var peak = InsightsCalculator.PeakWindowStart(buckets);

        Assert.Equal(9, peak);
    }

    [Fact]
    public void PeakWindow_NullWhenNoCompletions()
    {
        var buckets = Enumerable.Range(0, 24).Select(h => new HourBucket(h, 0)).ToList();
        Assert.Null(InsightsCalculator.PeakWindowStart(buckets));
    }
}
