using FlowState.Domain.Scoring;
using FlowState.Domain.Tasks;

namespace FlowState.UnitTests;

public class DecayProcessorTests
{
    [Fact]
    public void Process_MovesDecayedTask_ToDormant()
    {
        var task = new TaskItem("x", EnergyCost.Low);
        var p = DecayParameters.Default;
        var now = task.LastTouchedAt.AddDays(p.FreshnessHalfLifeDays * 3); // decayed out

        var result = DecayProcessor.Process(new[] { task }, now, p);

        Assert.Equal(1, result.WentDormant);
        Assert.Equal(DecayState.Dormant, task.DecayState);
        Assert.NotNull(task.DormantSince);
    }

    [Fact]
    public void Process_ResurfacesLongDormantTask()
    {
        var task = new TaskItem("x", EnergyCost.Low, Importance.High);
        var p = DecayParameters.Default;
        var dormantStart = DateTimeOffset.UtcNow;
        task.GoDormant(dormantStart);

        // Sweep well past the resurface threshold.
        var result = DecayProcessor.Process(new[] { task }, dormantStart.AddDays(p.HardResurfaceCeilingDays + 1), p);

        Assert.Equal(1, result.Resurfaced);
        Assert.Equal(DecayState.Resurfaced, task.DecayState);
    }

    [Fact]
    public void Process_LeavesFreshActiveTaskAlone()
    {
        var task = new TaskItem("fresh", EnergyCost.Low);
        var result = DecayProcessor.Process(new[] { task }, task.LastTouchedAt);

        Assert.Equal(0, result.WentDormant);
        Assert.Equal(0, result.Resurfaced);
        Assert.Equal(DecayState.Active, task.DecayState);
    }

    [Fact]
    public void Process_IgnoresCompletedTasks()
    {
        var task = new TaskItem("done", EnergyCost.Low);
        task.Complete();
        var now = task.LastTouchedAt.AddDays(60);

        var result = DecayProcessor.Process(new[] { task }, now);

        Assert.Equal(0, result.WentDormant);
        Assert.Equal(DecayState.Active, task.DecayState); // untouched lifecycle
    }

    [Fact]
    public void FullLifecycle_ActiveToDormantToResurfaced()
    {
        var task = new TaskItem("journey", EnergyCost.Low, Importance.High);
        var p = DecayParameters.Default;
        var t0 = task.LastTouchedAt;

        // 1. decays → dormant
        DecayProcessor.Process(new[] { task }, t0.AddDays(p.FreshnessHalfLifeDays * 3), p);
        Assert.Equal(DecayState.Dormant, task.DecayState);
        var dormantAt = task.DormantSince!.Value;

        // 2. ages → resurfaced
        DecayProcessor.Process(new[] { task }, dormantAt.AddDays(p.HardResurfaceCeilingDays + 1), p);
        Assert.Equal(DecayState.Resurfaced, task.DecayState);

        // 3. user acts → back to active
        task.Touch();
        Assert.Equal(DecayState.Active, task.DecayState);
    }
}
