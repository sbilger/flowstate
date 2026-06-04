# Research: ADHD Task-Management UX — what works vs. what apps get wrong

> **Sourcing note.** This is a structured synthesis of established research and the
> product landscape, written from prior knowledge — not a live literature search.
> Named bodies of work (Ebbinghaus, Gollwitzer's implementation intentions, Steel's
> Temporal Motivation Theory, Time Timer / time blindness, CBT-for-ADHD) are
> well-established. Claims marked **[verify]** are plausible but should be checked
> against primary sources before being used in interviews or marketing. Effect sizes
> and percentages are deliberately omitted unless well known, to avoid inventing stats.

---

## 1. Why generic task apps fail ADHD brains

Mainstream to-do apps are built around an assumption of intact executive function:
that the user can hold priorities in mind, choose what matters, estimate time, start
on demand, and tolerate delay. ADHD is, at its core, a disorder of **executive
function and reward/delay processing** — so the very abilities these apps assume are
the ones that are impaired. The result is a tool that increases load instead of
offloading it.

The relevant impairments (and the design pressure each creates):

| ADHD trait | Mechanism | What it does to a to-do app |
|---|---|---|
| **Working-memory deficit** | Can't hold items "in mind" | If it isn't captured instantly + visibly, it's gone |
| **Prioritization deficit** | Difficulty weighting competing options | A flat list of 40 items causes freeze, not action |
| **Time blindness** | Weak sense of duration/when ("now vs. not-now") | Deadlines feel unreal until they're emergencies |
| **Task-initiation deficit** | Trouble starting even wanted tasks | "Write report" is a wall; the app gives no on-ramp |
| **Delay aversion / dopamine** | Future rewards are heavily discounted | Long-payoff tasks lose to anything immediate |
| **Object impermanence** | Out of sight → out of mind | A backlog you don't scroll effectively ceases to exist |
| **Emotional dysregulation / RSD** | Shame spikes around failure | One broken streak → abandon the whole app |

**The throughline:** the failure mode isn't laziness, it's *load*. Every decision,
every scroll, every guilt cue is a tax an ADHD brain pays at a higher rate.

---

## 2. What the evidence supports (design principles)

Ordered roughly by strength of evidence + leverage.

### 2.1 Externalize memory; make capture frictionless
The single most robust ADHD coaching principle: **get it out of your head**.
Working memory is the bottleneck, so the tool's job is to be reliable external
storage. The cost of capture must approach zero — any friction and the thought is
lost (and trust in the system erodes, which is fatal: a system you don't trust, you
stop using).
- **Design:** instant, low-ceremony capture from anywhere; never block capture on
  categorization.

### 2.2 Reduce decisions — surface, don't list
Prioritization is the impaired skill, so **don't make the user do it cold**. Decision
load and "analysis paralysis" are repeatedly cited as why ADHD users bounce off
backlog apps. The highest-leverage move is to answer **"what do I do *now*?"** with
one answer, not a menu.
- **Design:** show one (or a few) chosen items; hide the full list by default.

### 2.3 Make time visible (fight time blindness)
The **Time Timer** (a visual analog countdown showing time as a shrinking colored
wedge) is a staple ADHD intervention precisely because it renders abstract duration
concrete. Pomodoro works for similar reasons: it converts "work for a while" into a
visible, bounded, finite block.
- **Design:** show estimated duration on tasks; a visible countdown during focus;
  "this is a 5-minute thing" framing lowers the initiation wall.

### 2.4 Lower the initiation wall — atomic next actions
A task like "file taxes" is inert because there's no obvious *first physical action*.
GTD's "next action" concept and the clinical tactic of "name the smallest first step"
both target initiation. Shrinking the unit of work is one of the most practical ADHD
levers.
- **Design:** break-it-down into checkable atomic steps; surface only the next step;
  "just do the first 2 minutes" framing.

### 2.5 Implementation intentions (if-then planning) — underused, strong evidence
Gollwitzer's **implementation intentions** ("if situation X arises, I will do Y") are
one of the better-evidenced behavior-change tools in psychology generally, and show
promise for ADHD/goal-completion specifically. Most consumer task apps capture *what*
but never *when/where/after-what* — which is exactly the part that drives follow-through.
- **Design:** optional "when/where" or "after I X" attached to a task; convert a vague
  task into an if-then. **[verify ADHD-specific effect sizes]**

### 2.6 Immediate, *novel* rewards
ADHD reward systems discount delayed payoff, so **immediate** feedback on completion
helps. The catch backed by experience: **novelty habituates fast** — a fixed
animation/streak that thrilled in week 1 is invisible by week 3. Variable/novel
reward holds attention longer (the same reason variable-ratio schedules are sticky).
- **Design:** celebrate completion immediately; *vary* the celebration; the
  "pick for me" dice is itself a small variable-reward dopamine hit.

### 2.7 Forgiving, shame-free design
Because of RSD and the abandonment cycle, **punishment backfires**. Punitive streaks
("you broke your 14-day streak!") convert one slip into total disengagement. Designs
that absorb misses without judgment retain users through the inevitable bad days.
- **Design:** no punitive streaks; resurfacing framed as a neutral decision, not a
  nag; "nothing rotted, here it is again."

### 2.8 Defeat object impermanence with resurfacing
"Out of sight, out of mind" means a silent backlog is functionally deleted. Things
must **come back into view** on their own, or they vanish. (This is the bridge to the
decay/resurfacing research — see the companion doc.)
- **Design:** avoided/ignored tasks resurface deliberately rather than sinking.

### 2.9 Body doubling
Working alongside another person (physically or virtually) to aid initiation/sustained
attention is widely reported in the ADHD community and increasingly studied. Evidence
is still **emerging/thin [verify]**, but the demand signal is strong (Focusmate, etc.).
- **Design:** future — live co-working/focus rooms (was idea A1).

### 2.10 Externalized structure beats willpower
The meta-principle: don't ask the ADHD brain to supply executive function on demand;
**bake the structure into the environment.** The app should *be* the prioritizer, the
timekeeper, the memory, the on-ramp.

> Worth stating plainly for the portfolio narrative: the strongest *clinical* evidence
> base for adult ADHD is **CBT/coaching and medication**, not apps. Digital tools are
> promising but under-studied. FlowState should be framed as a **scaffold that
> operationalizes coaching principles**, not a treatment. That's both honest and a
> better story.

---

## 3. The anti-patterns (what apps get wrong)

| Anti-pattern | Why it fails ADHD users | FlowState's stance |
|---|---|---|
| **The infinite backlog** | Becomes a guilt pile; freeze on open | Full list hidden by default; surface one thing |
| **Punitive streaks** | One miss → shame → abandonment | No punitive streaks; forgiving resurfacing |
| **Feature bloat / setup burden** | Configuration is itself executive load | Opinionated defaults; minimal setup |
| **Notification spam** | Banner blindness; anxiety; muted | One well-timed nudge, not many |
| **Forcing manual prioritization** | The impaired skill, demanded cold | Scoring engine prioritizes for you |
| **Flat list, no "what now"** | No on-ramp; paralysis | "What now?" hero is the home screen |
| **Rigid time/calendar models** | One missed day breaks the whole plan | Energy/state-based, not rigid schedule |
| **Static one-size reward** | Novelty habituates; feedback goes dead | Vary celebration; dice randomizer |
| **Over-gamification** | Childish/condescending; novelty fades | Calm by default; dopamine accents are deliberate |

---

## 4. FlowState scorecard — where we already align, and the gaps

**Already aligned (keep):**
- ✅ Frictionless capture (brain dump) → 2.1
- ✅ Surface one task, hide the list ("What now?") → 2.2
- ✅ Scoring engine prioritizes *for* the user → 2.2, 2.7
- ✅ Energy gate — never taunt with undoable work → 2.7 (shame reduction)
- ✅ "Pick for me" — kills decision paralysis, variable reward → 2.2, 2.6
- ✅ Focus timer → 2.3 (partial)
- ✅ Forgiving, calm visual design (no punitive streaks) → 2.7
- ✅ Decay/resurfacing planned → 2.8

**Gaps worth promoting in the roadmap:**
1. **Time made *visible*** (2.3). Today the focus timer counts *up*. A **visual
   countdown** (Time-Timer-style shrinking wedge) against the task's estimate would
   directly target time blindness. Cheap, high-leverage. → fold into Focus Mode (Slice 4).
2. **Atomic next-action / "just the first step"** (2.4). The break-it-down feature is
   roadmapped but the *initiation framing* ("do only the first 2 minutes") is a distinct,
   cheap win. → elevate within break-it-down.
3. **Implementation intentions** (2.5). Currently nothing captures *when/where/after-what*.
   An optional if-then field is well-evidenced and under-served by competitors —
   a genuine differentiator. → propose as a small new slice or fold into task detail.
4. **Varied completion reward** (2.6). One celebration will go stale. Rotate a small set.
   → cheap polish, fold into Focus/Done.
5. **"First step only" surfacing for high-cost tasks** when energy is low — instead of
   hiding a big task entirely, optionally surface *its smallest sub-step*. Ties scoring
   to initiation. → consider for Slice 3/4.

**Explicitly out of scope / be honest:** FlowState is a scaffold, not treatment. Don't
imply clinical efficacy.

---

## 5. Recommended priority changes (concrete)

| Change | Rationale | Cost | Suggested placement |
|---|---|---|---|
| Visual countdown in Focus Mode | Time blindness (2.3) — highest-leverage gap | Low | Slice 4 (Focus) |
| "First step / 2-minute" initiation framing | Initiation deficit (2.4) | Low | break-it-down |
| Optional if-then (implementation intention) field | Strong evidence, under-served (2.5) | Med | new mini-slice or task detail |
| Rotate completion celebration | Novelty habituation (2.6) | Low | Focus/Done |
| Keep resurfacing *non-punitive* (decision, not nag) | RSD/abandonment (2.7, 2.8) | — | Slice 3 design constraint |

These don't reorder the big slices; they sharpen *what each slice should contain* and
add one candidate mini-slice (implementation intentions).
