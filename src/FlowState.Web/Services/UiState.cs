using FlowState.Domain.Tasks;

namespace FlowState.Web.Services;

/// <summary>
/// Scoped UI state shared across components within a Blazor Server circuit —
/// holds the user's current energy level so the layout selector and the pages stay in sync.
/// </summary>
public class UiState
{
    public EnergyLevel Energy { get; private set; } = EnergyLevel.Good;

    public event Action? EnergyChanged;

    public void SetEnergy(EnergyLevel energy)
    {
        if (Energy == energy) return;
        Energy = energy;
        EnergyChanged?.Invoke();
    }

    public void CycleEnergy()
    {
        Energy = Energy switch
        {
            EnergyLevel.Good => EnergyLevel.Okay,
            EnergyLevel.Okay => EnergyLevel.Fried,
            _ => EnergyLevel.Good
        };
        EnergyChanged?.Invoke();
    }
}
