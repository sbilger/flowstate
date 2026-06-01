namespace FlowState.Domain.Tasks;

/// <summary>
/// How much energy a task demands. Drives the "energy gate" in the scoring engine:
/// when the user is low, high-cost tasks are filtered out of "What now?".
/// </summary>
public enum EnergyCost
{
    Low = 1,
    Medium = 2,
    High = 3
}
