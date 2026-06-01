# FlowState — The Scoring Engine (PriorityScore + Decay)

> This is the algorithmic heart of the app and the main "interesting problem" interview story.
> Spec first; implementation lands in Slice 3.

---

## Goal

Given all of a user's open tasks and their **current context** (energy level + time of day),
produce a ranked list so "What Now?" can surface the single best task. Plus a background
process so avoided tasks **decay** quietly and then **resurface** instead of rotting silently.

Two distinct mechanisms:
1. **PriorityScore** — a live ranking signal, recomputed on read and on a schedule.
2. **Decay / resurfacing** — a lifecycle that handles repeatedly-snoozed tasks humanely.

---

## Inputs (per task)

| Factor | Source | Intuition |
|---|---|---|
| `Importance` | user-set (Low/Med/High → 1/2/3) | matters more = ranks higher |
| `Age` | now − CreatedAt | older nags more, *up to a cap* |
| `SnoozeCount` | incremented on "Not now" | avoidance is a signal, handled specially |
| `EnergyMatch` | task EnergyCost vs current energy | don't surface 🔴 work when user is 🟢 |
| `TimeOfDayFit` | learned from completion history | favor tasks like those you finish at this hour |
| `DueProximity` | optional due date | deadlines pull score up sharply near due |

---

## PriorityScore formula (v1 — tunable)

```
score =  wImportance * ImportanceNorm
       + wAge        * AgeNorm            // capped so a task can't dominate forever
       + wDue        * DueProximityNorm
       + wTimeFit    * TimeOfDayFit
       - wSnooze     * SnoozePenalty      // mild: avoidance lowers live score…
       × EnergyGate                       // …but energy is a HARD multiplier/gate
```

- All factors normalized to 0..1 before weighting.
- **EnergyGate** is special: if the task's energy cost exceeds current energy, multiply by a
  small factor (e.g. 0.1) so it effectively drops out of "What Now?" but still exists.
  Exact-match energy gets a small boost.
- Default weights (starting point, tune later):
  `wImportance=0.35, wAge=0.20, wDue=0.25, wTimeFit=0.20, wSnooze=0.15`.
- Weights live in config so they're trivially adjustable (and unit-testable).

### Why a hard energy gate instead of just a weight?
The whole product promise is "never taunt me with work I can't do." A soft weight could still
float a 🔴 task to the top on a 🟢 day. The gate guarantees the promise. Good thing to explain
in an interview: *choosing constraints that encode product values, not just math.*

---

## Decay & resurfacing lifecycle

A task moves through `DecayState`:

```
ACTIVE ──(snoozed repeatedly / ignored N days)──▶ DECAYING ──(crosses threshold)──▶ RESURFACED
   ▲                                                                                    │
   └──────────────────────── user acts on it (start/complete/edit) ────────────────────┘
```

- **ACTIVE** — normal, ranked by PriorityScore.
- **DECAYING** — repeatedly snoozed or untouched; its live score is damped so it stops
  cluttering "What Now?". It's resting, not deleted.
- **RESURFACED** — once it's been decaying past a threshold (age + snooze), a background job
  flips it to RESURFACED and **reframes** it: instead of nagging, it asks a decision —
  *"This has waited 6 days. Knock it out, reschedule, or drop it?"*

The point: nothing silently rots in an unscrolled list, but avoided items also don't
permanently dominate. Avoidance becomes a *deliberate decision point*, not guilt.

---

## Background jobs (Hangfire, Slice 3)

| Job | Schedule | Does |
|---|---|---|
| `recalculate-priority` | every 15 min | refresh PriorityScore for active tasks |
| `resurface-stale` | hourly | move DECAYING→RESURFACED past threshold; create a decision prompt |
| (later) `dispatch-nudges` | every 5 min | send due nudges (Slice 6) |
| (later) `sync-integrations` | every 10 min | poll email/calendar (Slice 8) |

Note: PriorityScore is also computed on read for freshness; the job keeps stored scores warm
for sorting/insights without recomputing the world on every request.

---

## "What Now?" selection (Slice 1–2 uses a simple version; full in Slice 3)

```
candidates = tasks.Where(Status == Open && EnergyGate(currentEnergy) > floor)
top        = candidates.OrderByDescending(PriorityScore).First()
"pick for me" = weightedRandom(candidates, weight: PriorityScore)
```

- Surfacing = deterministic top pick.
- "Pick for me" = weighted random so it *usually* picks something good but adds delightful variance.

---

## Testability (why this is great for a portfolio)

The scoring engine is **pure, deterministic, and side-effect free** → ideal for xUnit:
- Energy gate excludes over-budget tasks. ✅
- Higher importance outranks lower, all else equal. ✅
- Age increases score up to the cap, not beyond. ✅
- Snooze damps score; repeated snooze → DECAYING. ✅
- Resurfacing fires exactly at threshold. ✅
- "Pick for me" distribution roughly tracks weights (statistical test). ✅

This is the test suite to show off. Clean inputs → clear assertions.
