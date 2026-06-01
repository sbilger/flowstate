using FlowState.Domain.Tasks;

namespace FlowState.UnitTests;

public class TaskItemTests
{
    [Fact]
    public void NewTask_DefaultsToOpen_AndStampsCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;

        var task = new TaskItem("Fix the export bug", EnergyCost.High);

        Assert.Equal(TaskItemStatus.Open, task.Status);
        Assert.True(task.CreatedAt >= before);
        Assert.Null(task.CompletedAt);
        Assert.NotEqual(Guid.Empty, task.Id);
    }

    [Fact]
    public void NewTask_TrimsTitle()
    {
        var task = new TaskItem("  reply to landlord  ", EnergyCost.Low);
        Assert.Equal("reply to landlord", task.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void NewTask_RejectsEmptyTitle(string? title)
    {
        Assert.Throws<ArgumentException>(() => new TaskItem(title!, EnergyCost.Medium));
    }

    [Fact]
    public void Complete_MarksDone_AndStampsCompletedAt()
    {
        var task = new TaskItem("Gather W-2s", EnergyCost.Medium);

        task.Complete();

        Assert.Equal(TaskItemStatus.Done, task.Status);
        Assert.NotNull(task.CompletedAt);
    }
}
