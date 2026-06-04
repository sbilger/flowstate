# FlowState Research — synthesis & decisions

Two deep-dives, plus how they change the plan.

| Doc | What it covers |
|---|---|
| [adhd-task-ux.md](adhd-task-ux.md) | Evidence-based ADHD task-management UX: what works, the anti-patterns, FlowState scorecard |
| [decay-resurfacing.md](decay-resurfacing.md) | Prior art for the Slice 3 engine: spaced repetition, feed decay, **OS scheduler aging**, snooze psychology |

> **Honesty note:** these are syntheses from prior knowledge, not a live web search.
> Well-established work is named; thinner claims are tagged **[verify]**. Verify
> specific empirical/effect-size claims against primary sources before they go into
> interview talking points or marketing copy.

---

## The two ideas worth remembering

1. **The failure mode of ADHD task apps is *load*, not laziness.** Every decision,
   scroll, and guilt cue is a tax. FlowState's job is to *be* the executive function —
   prioritizer, timekeeper, memory, on-ramp — not to demand it.

2. **Decay vs. starvation is the core algorithm tension.** Tasks should fade out of
   view (feed-style decay) but must not rot silently (object impermanence). Resolve it
   with **OS-scheduler-style aging**: sink for a while, then aging forces resurfacing
   as a non-punitive decision. *Sink, then surface — never sink forever.*

---

## Concrete changes to the plan

### Validated (keep doing)
Frictionless capture · surface-one-thing · scoring prioritizes for you · energy gate ·
"pick for me" · forgiving/calm design · planned decay-resurfacing. All map to evidence.

### Sharpened (change what a slice *contains*)
- **Focus Mode (Slice 4):** add a **visual countdown** (Time-Timer-style), not just a
  count-up timer → directly targets time blindness. Highest-leverage gap found.
- **Break-it-down:** add **"just the first 2 minutes" initiation framing** + surface the
  smallest sub-step → targets task-initiation deficit.
- **Completion reward:** **rotate/vary** the celebration → novelty habituation is real.
- **Slice 3 design constraint:** resurfacing is a **forcing decision** (do / reschedule /
  break down / **drop**), never a nag. "Drop it" is first-class (closes the loop, no shame).

### New candidate mini-slice
- **Implementation intentions (if-then planning):** an optional "when / where / after I X"
  field. Strong general evidence (Gollwitzer), under-served by competitors → real
  differentiator. Small build.

### Slice 3 algorithm (locked direction)
Add to the existing `TaskScorer`: exponential **freshness decay** (tunable half-life),
a **DecayState** lifecycle (ACTIVE → DORMANT → RESURFACED), and **aging/anti-starvation**
borrowed from OS schedulers, driven by **Hangfire** recurring jobs, ending in a
**resurfacing decision UI**. Slice-2's `SnoozeCount`/`LastSnoozedAt` already feed it.
Start parameters and what-NOT-to-build are in decay-resurfacing.md §4.5–4.6.

### Honesty guardrail (portfolio + product)
FlowState is a **scaffold that operationalizes coaching principles**, not a treatment.
The strongest clinical evidence for adult ADHD is CBT/coaching + medication. Framing it
honestly is both more defensible and a better story.
