using FlowState.Domain.Scoring;
using FlowState.Domain.Tasks;

namespace FlowState.UnitTests;

public class TaskScorerTests
{
    private static readonly DateTimeOffset Morning =
        new(2026, 6, 1, 9, 0, 0, TimeSpan.Zero); // 9am — high-energy work fits

    [Fact]
    public void EnergyGate_ExcludesOverBudgetTasks()
    {
        // High-cost task, user is Fried → gated far down.
        var gate = TaskScorer.EnergyGate(EnergyCost.High, EnergyLevel.Fried, ScoringWeights.Default);
        Assert.True(gate < 0.1);
    }

    [Fact]
    public void EnergyGate_BoostsExactMatch()
    {
        var gate = TaskScorer.EnergyGate(EnergyCost.Low, EnergyLevel.Fried, ScoringWeights.Default);
        Assert.True(gate > 1.0);
    }

    [Fact]
    public void EnergyGate_PassesWithinBudget()
    {
        var gate = TaskScorer.EnergyGate(EnergyCost.Low, EnergyLevel.Good, ScoringWeights.Default);
        Assert.Equal(1.0, gate, 3);
    }

    [Fact]
    public void PassesEnergyGate_TrueWhenWithinBudget()
    {
        Assert.True(TaskScorer.PassesEnergyGate(EnergyCost.Medium, EnergyLevel.Good));
        Assert.True(TaskScorer.PassesEnergyGate(EnergyCost.Medium, EnergyLevel.Okay));
        Assert.False(TaskScorer.PassesEnergyGate(EnergyCost.High, EnergyLevel.Okay));
    }

    [Fact]
    public void HigherImportance_OutranksLower_AllElseEqual()
    {
        var high = new TaskItem("a", EnergyCost.Low, Importance.High);
        var low = new TaskItem("b", EnergyCost.Low, Importance.Low);

        var hs = TaskScorer.Score(high, EnergyLevel.Good, Morning);
        var ls = TaskScorer.Score(low, EnergyLevel.Good, Morning);

        Assert.True(hs > ls);
    }

    [Fact]
    public void OlderTask_OutranksNewer_AllElseEqual()
    {
        var newer = new TaskItem("new", EnergyCost.Low, Importance.Normal);
        var older = new TaskItem("old", EnergyCost.Low, Importance.Normal);

        // Score the same tasks at two different "now"s: one far in the future makes them "older".
        var asNew = TaskScorer.Score(newer, EnergyLevel.Good, newer.CreatedAt.AddMinutes(1));
        var asOld = TaskScorer.Score(older, EnergyLevel.Good, older.CreatedAt.AddDays(5));

        Assert.True(asOld > asNew);
    }

    [Fact]
    public void AgeFactor_SaturatesAtCap()
    {
        var task = new TaskItem("x", EnergyCost.Low, Importance.Normal);
        var weights = ScoringWeights.Default;

        var atCap = TaskScorer.Score(task, EnergyLevel.Good, task.CreatedAt.AddDays(weights.AgeSaturationDays));
        var wayPast = TaskScorer.Score(task, EnergyLevel.Good, task.CreatedAt.AddDays(weights.AgeSaturationDays * 4));

        // Beyond the cap, age contributes no extra — scores equal (within rounding).
        Assert.Equal(atCap, wayPast, 4);
    }

    [Fact]
    public void Snoozing_LowersScore()
    {
        var fresh = new TaskItem("fresh", EnergyCost.Low, Importance.Normal);
        var snoozed = new TaskItem("snoozed", EnergyCost.Low, Importance.Normal);
        snoozed.Snooze();
        snoozed.Snooze();
        snoozed.Snooze();

        var fs = TaskScorer.Score(fresh, EnergyLevel.Good, Morning);
        var ss = TaskScorer.Score(snoozed, EnergyLevel.Good, Morning);

        Assert.True(ss < fs);
    }
}
