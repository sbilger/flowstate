using FlowState.Domain.Focus;

namespace FlowState.UnitTests;

public class FocusSessionTests
{
    [Fact]
    public void New_Session_StartsInProgress()
    {
        var s = new FocusSession(Guid.NewGuid(), "task", TimeSpan.FromMinutes(15));
        Assert.Equal(FocusOutcome.InProgress, s.Outcome);
        Assert.Null(s.EndedAt);
        Assert.Equal(TimeSpan.FromMinutes(15), s.PlannedDuration);
    }

    [Fact]
    public void New_Session_RejectsNonPositiveDuration()
    {
        Assert.Throws<ArgumentException>(() =>
            new FocusSession(Guid.NewGuid(), "task", TimeSpan.Zero));
    }

    [Fact]
    public void Elapsed_GrowsWhileRunning_FreezesAfterEnd()
    {
        var s = new FocusSession(Guid.NewGuid(), "task", TimeSpan.FromMinutes(15));
        var t1 = s.StartedAt.AddMinutes(5);
        Assert.Equal(TimeSpan.FromMinutes(5), s.Elapsed(t1));

        s.End(FocusOutcome.Completed);
        var frozen = s.Elapsed(s.StartedAt.AddMinutes(99));
        Assert.True(frozen < TimeSpan.FromMinutes(99)); // frozen at end, not 99 min
    }

    [Fact]
    public void End_SetsOutcome_AndIsIdempotent()
    {
        var s = new FocusSession(Guid.NewGuid(), "task", TimeSpan.FromMinutes(15));
        s.End(FocusOutcome.Completed);
        Assert.Equal(FocusOutcome.Completed, s.Outcome);
        Assert.NotNull(s.EndedAt);

        var firstEnd = s.EndedAt;
        s.End(FocusOutcome.Abandoned); // no-op once ended
        Assert.Equal(FocusOutcome.Completed, s.Outcome);
        Assert.Equal(firstEnd, s.EndedAt);
    }

    [Fact]
    public void End_WithInProgress_TreatedAsAbandoned()
    {
        var s = new FocusSession(Guid.NewGuid(), "task", TimeSpan.FromMinutes(15));
        s.End(FocusOutcome.InProgress);
        Assert.Equal(FocusOutcome.Abandoned, s.Outcome);
    }
}
