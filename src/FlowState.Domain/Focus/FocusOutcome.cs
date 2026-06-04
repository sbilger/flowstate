namespace FlowState.Domain.Focus;

/// <summary>How a focus session ended — logged for Insights.</summary>
public enum FocusOutcome
{
    /// <summary>Session still running (not yet ended).</summary>
    InProgress = 0,
    /// <summary>User marked the task done.</summary>
    Completed = 1,
    /// <summary>User made progress but didn't finish.</summary>
    Progress = 2,
    /// <summary>User bailed / got stuck.</summary>
    Abandoned = 3
}
