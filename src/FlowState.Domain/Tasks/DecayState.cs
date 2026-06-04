namespace FlowState.Domain.Tasks;

/// <summary>
/// Where a task sits in the decay/resurfacing lifecycle.
///
///   ACTIVE ──decays / repeatedly snoozed──▶ DORMANT ──aging crosses threshold──▶ RESURFACED
///      ▲                                                                              │
///      └──────────────── user acts (start / complete / edit / reschedule / drop) ─────┘
///
/// Decay declutters "What now?"; aging (borrowed from OS schedulers) guarantees no task
/// starves — a neglected task's pressure climbs until the system forces a decision.
/// </summary>
public enum DecayState
{
    /// <summary>Normal. Ranked by PriorityScore and eligible for "What now?".</summary>
    Active = 0,

    /// <summary>Decayed/avoided. Damped out of "What now?" so it stops cluttering — resting, not deleted.</summary>
    Dormant = 1,

    /// <summary>Aging forced it back as a decision: do / reschedule / break down / drop.</summary>
    Resurfaced = 2
}
