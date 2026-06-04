using FlowState.Domain.Scoring;
using FlowState.Domain.Tasks;

namespace FlowState.UnitTests;

public class DecayEngineTests
{
    [Fact]
    public void Freshness_IsOne_WhenJustTouched()
    {
        var task = new TaskItem("fresh", EnergyCost.Low);
        var f = DecayEngine.Freshness(task, task.LastTouchedAt);
        Assert.Equal(1.0, f, 3);
    }

    [Fact]
    public void Freshness_HalvesAtHalfLife()
    {
        var task = new TaskItem("x", EnergyCost.Low);
        var p = DecayParameters.Default;
        var atHalfLife = DecayEngine.Freshness(task, task.LastTouchedAt.AddDays(p.FreshnessHalfLifeDays), p);
        Assert.Equal(0.5, atHalfLife, 2);
    }

    [Fact]
    public void Freshness_DecaysTowardZero()
    {
        var task = new TaskItem("x", EnergyCost.Low);
        var p = DecayParameters.Default;
        var farFuture = DecayEngine.Freshness(task, task.LastTouchedAt.AddDays(p.FreshnessHalfLifeDays * 10), p);
        Assert.True(farFuture < 0.01);
    }

    [Fact]
    public void ShouldGoDormant_WhenDecayedPastThreshold()
    {
        var task = new TaskItem("x", EnergyCost.Low);
        var p = DecayParameters.Default;
        // Far enough out that freshness < threshold.
        var later = task.LastTouchedAt.AddDays(p.FreshnessHalfLifeDays * 3);
        Assert.True(DecayEngine.ShouldGoDormant(task, later, p));
    }

    [Fact]
    public void ShouldGoDormant_WhenSnoozedEnough_EvenIfFresh()
    {
        var task = new TaskItem("x", EnergyCost.Low);
        task.Snooze(); task.Snooze(); task.Snooze(); // hits the snooze threshold
        // Snoozing also touches, so freshness is high — dormancy must trigger on snooze count.
        Assert.True(DecayEngine.ShouldGoDormant(task, task.LastTouchedAt, DecayParameters.Default));
    }

    [Fact]
    public void ShouldGoDormant_False_ForFreshUnsnoozedTask()
    {
        var task = new TaskItem("x", EnergyCost.Low);
        Assert.False(DecayEngine.ShouldGoDormant(task, task.LastTouchedAt, DecayParameters.Default));
    }

    [Fact]
    public void AgingPressure_GrowsWithDormancy()
    {
        var task = new TaskItem("x", EnergyCost.Low);
        var now = DateTimeOffset.UtcNow;
        task.GoDormant(now);

        var early = DecayEngine.AgingPressure(task, now.AddDays(1));
        var late = DecayEngine.AgingPressure(task, now.AddDays(5));
        Assert.True(late > early);
    }

    [Fact]
    public void AgingPressure_HigherForImportantTasks()
    {
        var now = DateTimeOffset.UtcNow;
        var high = new TaskItem("h", EnergyCost.Low, Importance.High);
        var low = new TaskItem("l", EnergyCost.Low, Importance.Low);
        high.GoDormant(now);
        low.GoDormant(now);

        var hp = DecayEngine.AgingPressure(high, now.AddDays(3));
        var lp = DecayEngine.AgingPressure(low, now.AddDays(3));
        Assert.True(hp > lp);
    }

    [Fact]
    public void ShouldResurface_WhenHardCeilingHit()
    {
        var task = new TaskItem("x", EnergyCost.Low, Importance.Low);
        var now = DateTimeOffset.UtcNow;
        task.GoDormant(now);
        var p = DecayParameters.Default;

        // Even a low-importance task must resurface by the hard ceiling — anti-starvation guarantee.
        Assert.True(DecayEngine.ShouldResurface(task, now.AddDays(p.HardResurfaceCeilingDays + 1), p));
    }

    [Fact]
    public void ShouldResurface_False_ForActiveTask()
    {
        var task = new TaskItem("x", EnergyCost.Low);
        Assert.False(DecayEngine.ShouldResurface(task, DateTimeOffset.UtcNow.AddDays(30)));
    }

    [Fact]
    public void Touch_ReturnsDormantTaskToActive()
    {
        var task = new TaskItem("x", EnergyCost.Low);
        task.GoDormant(DateTimeOffset.UtcNow);
        Assert.Equal(DecayState.Dormant, task.DecayState);

        task.Touch();
        Assert.Equal(DecayState.Active, task.DecayState);
        Assert.Null(task.DormantSince);
    }
}
