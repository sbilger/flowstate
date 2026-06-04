# Research: Decay, Resurfacing & Snooze — prior art for the Slice 3 engine

> **Sourcing note.** Synthesis from prior knowledge, not a live search. Named systems
> and effects (Ebbinghaus forgetting curve, Leitner, SM-2/SuperMemo, Anki, FSRS,
> Duolingo Half-Life Regression, Hacker News/Reddit ranking, OS priority aging,
> Zeigarnik effect, Steel's Temporal Motivation Theory) are real and well-documented;
> verify exact formulas/parameters against primary sources before quoting. The goal is
> to steal the *right* ideas for FlowState's decay/resurfacing, not to reimplement a
> spaced-repetition scheduler.

---

## 1. The problem we're modeling

Two distinct dynamics, often conflated:

1. **Freshness decay** — a task's salience should fade if untouched, so it stops
   cluttering "What now?". (Otherwise everything competes forever.)
2. **Anti-starvation resurfacing** — but a faded task must *not* silently rot
   (object impermanence, §2.8 of the UX doc). It has to come back as a **decision**.

The art is balancing these: decay enough to declutter, resurface enough to stay honest.
A naive "lowest priority sinks forever" model fails ADHD users specifically.

---

## 2. Prior art, four domains

### 2.1 Spaced repetition (memory scheduling)
The richest prior art on "when should this resurface?"

- **Ebbinghaus forgetting curve** — retention decays roughly exponentially with time;
  review resets/flattens it. The canonical "things fade unless touched" model.
- **Leitner system** — physical boxes; correct answers move a card to a
  longer-interval box, wrong answers send it back to daily. Simple, robust, *discrete*.
- **SM-2 (SuperMemo) / Anki** — per-item ease factor + interval that grows on success.
- **FSRS (Free Spaced Repetition Scheduler)** — modern, models memory as
  stability + retrievability; schedules review at a target recall probability.
- **Duolingo Half-Life Regression** — predicts a word's "half-life" and reviews near
  the point of forgetting.

**Steal:** the *exponential decay + scheduled resurfacing* shape, and the idea of a
tunable **half-life**. **Leave:** per-item difficulty modeling and recall-probability
optimization — overkill for tasks (we're not optimizing memory, we're preventing rot).

### 2.2 Content ranking decay (recommenders / feeds)
How ranked feeds keep fresh things up and let stale things fall — *without* deleting.

- **Hacker News:** `score ≈ (points − 1) / (age_hours + 2)^gravity` (gravity ≈ 1.8).
  Importance (points) fights time (age); gravity controls how fast things sink.
- **Reddit "hot":** log(votes) + time term — early votes dominate; age erodes.
- **General recommenders:** exponential time-decay weighting of relevance.

**Steal:** the **importance-vs-age tension** — a high-importance task should resist
decay longer than a trivial one (our score already does a soft version of this). The
HN gravity exponent is a clean mental model for "how fast does an untouched task sink."

### 2.3 OS scheduler aging (the best analogy — and best interview story)
CPU schedulers face the *exact* problem: low-priority processes could **starve**
forever behind high-priority ones. The fix is **aging**: a process's priority is
*raised the longer it waits*, guaranteeing it eventually runs. (Multi-level feedback
queues do this explicitly.)

**This is FlowState's anti-starvation mechanism, named.** An avoided task can't be
allowed to starve; its effective priority must climb with neglect until the system
*forces* attention. This is a genuinely strong "I borrowed a CS concept and applied it
to human behavior" talking point — concrete, correct, and memorable.

**Steal:** **aging guarantees no task starves.** Decay declutters *now*, but aging
pushes a long-ignored task back up until it's force-resurfaced.

> Note the productive tension: §2.1/2.2 say "untouched → sinks," §2.3 says "untouched →
> eventually rises." FlowState resolves it with a **lifecycle**: tasks decay (sink)
> for a while, then aging takes over and resurfaces them as a forced decision. Sink,
> then surface — not sink forever.

### 2.4 Snooze / defer patterns (product prior art)
- **Gmail/Inbox snooze, Boomerang:** hide until a chosen time, then reappear at top.
- Observed failure mode: **snooze becomes an avoidance loop** — the same item snoozed
  again and again, never done. Repeated snoozing is itself the signal.

**Steal:** treat **snooze count as a first-class avoidance signal** (we already track
`SnoozeCount`), and convert repeated snoozing into a *forcing decision* rather than an
infinite defer.

---

## 3. The psychology

- **Zeigarnik effect** — open/unfinished tasks occupy attention (intrusive "loops").
  Implication: a silent backlog isn't free; it's low-grade background stress. Better to
  *close the loop* — even "decide to drop it" is a resolution that frees attention.
- **Steel's Temporal Motivation Theory** — Motivation ≈ (Expectancy × Value) /
  (Impulsiveness × Delay). Reads almost directly onto a task score:
  - *Value* → importance (raise score)
  - *Delay* → deadline proximity / age (handled by aging)
  - *Impulsiveness × aversiveness* → why we snooze (snooze penalty, but **bounded**, or
    avoidance compounds into permanent burial — exactly what aging must counteract).
- **Procrastination = task aversiveness × low initiation**, not a time-management gap.
  So resurfacing should reduce aversiveness (reframe, shrink, offer "drop") rather than
  nag harder.
- **Forcing functions / decision points** — indefinite deferral thrives on the absence
  of a decision moment. GTD's **weekly review** and **tickler file** are forcing
  functions: everything resurfaces for an explicit keep/defer/drop. A scheduled
  decision point is what breaks the snooze loop.

---

## 4. Design synthesis for FlowState Slice 3

### 4.1 Two signals, one lifecycle
- **Freshness** = exponential decay since last touch: `freshness = exp(−λ · daysSinceTouched)`,
  with a tunable **half-life** (e.g. 5 days → λ = ln(2)/5). "Touched" = created,
  surfaced-and-acted, edited, or snoozed.
- **Avoidance** = `SnoozeCount` (+ recency of last snooze).
- **Lifecycle state** (`DecayState`):
  ```
  ACTIVE ──decays / repeatedly snoozed──▶ DORMANT ──aging crosses threshold──▶ RESURFACED
     ▲                                                                              │
     └──────────────── user acts (start / complete / edit / reschedule / drop) ─────┘
  ```
  - **ACTIVE** — normal; ranked by PriorityScore.
  - **DORMANT** — decayed; damped out of "What now?" so it stops cluttering. Resting, not deleted.
  - **RESURFACED** — aging forced it back as a **decision**: *"This has waited 6 days —
    knock it out, reschedule, break it down, or drop it?"* Non-punitive (UX §2.7).

### 4.2 Aging (anti-starvation) — the key addition
Borrowing OS scheduler aging: while DORMANT, a task accrues an **age pressure** term
that grows with neglect. When `agePressure × importance` crosses a threshold, flip to
RESURFACED regardless of how low freshness sank. **Guarantee: no task starves.** High
importance crosses sooner; trivial items can wait — but everything eventually surfaces.

### 4.3 Resurfacing as a forcing decision, not a nag
RESURFACED tasks present a **closed-loop choice** (Zeigarnik + forcing function):
**Do it · Reschedule · Break it down · Drop it.** "Drop it" is first-class — letting a
user *consciously* delete relieves the open loop without shame. This is the mechanism
that breaks the snooze cycle.

### 4.4 Reconciling with the existing TaskScorer (from Slice 2)
We already have importance + age + time-of-day − snooze, × energy gate. Slice 3 adds:
- a **freshness/decay** multiplier (exponential, half-life-tunable),
- a **DormancyState** lifecycle + the **aging/anti-starvation** override,
- a **Hangfire** recurring job (`recalculate-priority`, `resurface-dormant`),
- the **resurfacing decision UI**.
The Slice-2 `SnoozeCount` and `LastSnoozedAt` fields already feed this — they were
added with Slice 3 in mind.

### 4.5 Suggested starting parameters (tune later; keep in `ScoringWeights`-style config)
| Parameter | Start | Meaning |
|---|---|---|
| Freshness half-life | 5 days | How fast an untouched task decays out of view |
| Dormancy threshold | freshness < 0.25 **or** snoozeCount ≥ 3 | When a task goes to rest |
| Aging rate | linear, ~0.1/day of dormancy | How fast neglect rebuilds pressure |
| Resurface trigger | agePressure × importanceNorm ≥ 1.0 | Forced decision point |
| Hard ceiling | resurface by 14 days dormant regardless | Absolute anti-starvation guarantee |

### 4.6 What to deliberately NOT build
- ❌ SM-2 / FSRS-grade per-item difficulty + recall modeling — wrong tool; tasks aren't flashcards.
- ❌ Punitive framing on resurfacing — violates UX §2.7.
- ❌ Infinite/silent hiding — the whole point is the guarantee that nothing rots.
- ❌ Snooze with no consequence — repeated snooze must escalate toward a decision.

---

## 5. The interview narrative this unlocks

> "Avoided tasks shouldn't rot silently, but they also shouldn't dominate forever. I
> modeled it as a lifecycle: tasks **decay** out of view like content in a ranked feed
> (exponential freshness, tunable half-life), but I borrowed **priority aging from OS
> schedulers** so a neglected task can't *starve* — its pressure climbs until the system
> forces a decision. The resurfacing moment is a deliberate forcing function — *do it,
> reschedule, break down, or drop* — which closes the open loop (Zeigarnik) without the
> shame that makes ADHD users abandon apps."

That's three distinct, correct technical references (feed decay, OS aging, a behavioral
forcing function) tied to a real product constraint — exactly the kind of "interesting
problem I solved" answer that lands.
