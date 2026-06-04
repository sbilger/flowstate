namespace FlowState.Domain.Tasks;

/// <summary>
/// The user's *current* energy state (distinct from a task's EnergyCost).
/// Drives the energy gate: a task is only surfaced when the user has enough
/// energy to take it on.
/// </summary>
public enum EnergyLevel
{
    Fried = 1,   // low — only the easy wins
    Okay = 2,    // medium gas in the tank
    Good = 3     // sharp; anything goes
}
