using FlowState.Domain.Scoring;
using FlowState.Domain.Tasks;

namespace FlowState.UnitTests;

public class WhatNowSelectorTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 1, 9, 0, 0, TimeSpan.Zero);

    private static List<TaskItem> Sample() => new()
    {
        new TaskItem("high scary", EnergyCost.High, Importance.High),
        new TaskItem("medium thing", EnergyCost.Medium, Importance.Normal),
        new TaskItem("easy win", EnergyCost.Low, Importance.Low),
    };

    [Fact]
    public void Rank_WhenFried_ExcludesHighAndMediumCost()
    {
        var ranked = WhatNowSelector.Rank(Sample(), EnergyLevel.Fried, Now);

        Assert.Single(ranked);
        Assert.Equal("easy win", ranked[0].Task.Title);
    }

    [Fact]
    public void Rank_WhenGood_IncludesEverythingOpen()
    {
        var ranked = WhatNowSelector.Rank(Sample(), EnergyLevel.Good, Now);
        Assert.Equal(3, ranked.Count);
    }

    [Fact]
    public void Rank_ExcludesCompletedTasks()
    {
        var tasks = Sample();
        tasks[2].Complete(); // complete the easy win

        var ranked = WhatNowSelector.Rank(tasks, EnergyLevel.Good, Now);

        Assert.DoesNotContain(ranked, s => s.Task.Title == "easy win");
        Assert.Equal(2, ranked.Count);
    }

    [Fact]
    public void Top_ReturnsHighestScored()
    {
        // Good energy in the morning → the high-importance high-cost task should top.
        var top = WhatNowSelector.Top(Sample(), EnergyLevel.Good, Now);
        Assert.NotNull(top);
        Assert.Equal("high scary", top!.Task.Title);
    }

    [Fact]
    public void Top_ReturnsNull_WhenNothingFitsEnergy()
    {
        var onlyHard = new List<TaskItem> { new("hard", EnergyCost.High, Importance.High) };
        var top = WhatNowSelector.Top(onlyHard, EnergyLevel.Fried, Now);
        Assert.Null(top);
    }

    [Fact]
    public void PickForMe_AlwaysReturnsEligibleTask()
    {
        var rng = new Random(42);
        for (int i = 0; i < 50; i++)
        {
            var pick = WhatNowSelector.PickForMe(Sample(), EnergyLevel.Fried, Now, rng);
            Assert.NotNull(pick);
            // Fried → only the low-cost task is eligible.
            Assert.Equal("easy win", pick!.Task.Title);
        }
    }

    [Fact]
    public void PickForMe_DistributionFavorsHigherScores()
    {
        // With Good energy, over many rolls the top-scoring task should be picked most often.
        var rng = new Random(7);
        var counts = new Dictionary<string, int>();
        for (int i = 0; i < 600; i++)
        {
            var pick = WhatNowSelector.PickForMe(Sample(), EnergyLevel.Good, Now, rng)!;
            counts[pick.Task.Title] = counts.GetValueOrDefault(pick.Task.Title) + 1;
        }

        // The high-importance task should be picked more than the low-importance easy win.
        Assert.True(counts["high scary"] > counts["easy win"]);
    }
}
