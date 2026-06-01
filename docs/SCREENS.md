# FlowState — Screen Specs & Wireframes

> Text wireframes for each screen. These are the blueprint for the visual-design phase.
> ASCII layouts show structure/hierarchy, not final styling.

**Design principles (ADHD-first):**
- **One primary action per screen.** Never make the user choose between many things.
- **The full list is hidden by default.** Surfacing > browsing.
- **Calm, low-stimulation default; dopamine moments are deliberate** (completion, "pick for me").
- **Always answer "what do I do next?"** in one glance.

---

## 0. App shell / navigation

```
┌────────────────────────────────────────────────┐
│  FlowState          🟢 Energy: Good   ▾   ☰     │  ← top bar: brand • energy selector • menu
├────────────────────────────────────────────────┤
│                                                  │
│                 [ screen content ]               │
│                                                  │
├────────────────────────────────────────────────┤
│   ⚡ Now    📥 Inbox    📋 Tasks    📊 Insights   │  ← bottom nav (mobile) / side nav (desktop)
└────────────────────────────────────────────────┘
```
- **Energy selector** is global (top bar) — changes what every screen shows.
- Bottom nav on mobile/PWA, left rail on desktop. 4 destinations only.

---

## 1. Today / "What Now?" — the hero screen

```
┌────────────────────────────────────────────────┐
│  Good morning, Sean.        🟢 Energy: Good ▾   │
│  You're sharp right now — here's the one thing.  │
│                                                  │
│   ┌──────────────────────────────────────────┐  │
│   │  🔴 HIGH FOCUS                            │  │
│   │                                           │  │
│   │  Fix the export bug                       │  │
│   │  ~30 min · from: brain dump               │  │
│   │                                           │  │
│   │     [  ▶ Start Focus  ]   [ Not now ]     │  │
│   └──────────────────────────────────────────┘  │
│                                                  │
│   Don't like it?   [ 🎲 Pick for me ]            │
│                                                  │
│   ── next up (if you finish) ──────────────────  │
│   • Reply to landlord            🟢 5 min        │
│   • Gather W-2s                  🟡 15 min       │
│                                                  │
│   🗓 Free until 2:00pm (next: Dentist 2:30)     │
└────────────────────────────────────────────────┘
```
- **The card is the product.** One task, big, with the obvious action.
- "Not now" = snooze (increments SnoozeCount, feeds decay).
- "Pick for me" = weighted-random reroll (dopamine).
- Calendar strip at bottom = free/busy awareness.
- Empty state: *"Nothing queued. Brain dump something →"* with a single capture box.

---

## 2. Focus Mode — full-screen, distraction-free

```
┌────────────────────────────────────────────────┐
│                                          ✕ end  │
│                                                  │
│                Fix the export bug                │
│                                                  │
│                   ⏱ 12:47                        │  ← live timer (SignalR)
│                  ───────────                     │
│                                                  │
│              [ ⏸ Pause ]   [ ✓ Done ]            │
│                                                  │
│         "Stay here. One thing. You've got it."   │
│                                                  │
└────────────────────────────────────────────────┘
```
- Everything else disappears. No nav, no list.
- On Done → quick outcome log (Finished / Made progress / Stuck) → confetti moment → back to "What Now?".
- Timer state survives refresh (server-side via SignalR).

---

## 3. Brain Dump / All Tasks

```
┌────────────────────────────────────────────────┐
│  📋 Tasks                          + Brain dump  │
│  ┌──────────────────────────────────────────┐   │
│  │ Type anything and hit enter…             │   │  ← always-focused capture box
│  └──────────────────────────────────────────┘   │
│                                                  │
│  Filter:  [ All ]  🟢 Low  🟡 Med  🔴 High       │
│                                                  │
│  🔴 Fix the export bug          ~30m   ⋯         │
│  🟡 Gather W-2s                 ~15m   ⋯         │
│  🟢 Reply to landlord           ~5m    ⋯         │
│  🟢 Buy disc golf basket        ~10m   ⋯         │
│  🟡 Plan weekend league         ~20m   ⋯         │
│                                                  │
│        … 12 more  (tap to expand)               │  ← full list collapsed by default
└────────────────────────────────────────────────┘
```
- Capture is **instant and frictionless** — type, enter, gone. Energy auto-guessed, editable later.
- Full list deliberately de-emphasized; this is the "I need to see everything" escape hatch, not the default.

---

## 4. Task Detail (+ Break it down)

```
┌────────────────────────────────────────────────┐
│  ← back                                    ⋯     │
│                                                  │
│  File taxes                                      │
│  Energy: [🟢] [🟡] [🔴]    Est: [ 45m ]          │
│  Tags: #finance #annual                          │
│                                                  │
│  Notes ─────────────────────────────────────    │
│  │ deadline April 15                         │   │
│                                                  │
│  [ 🧩 Break it down ]                            │  ← rule-based now, LLM later
│                                                  │
│  Sub-steps ─────────────────────────────────    │
│   ☐ Find last year's return        🟢           │
│   ☐ Gather W-2s                    🟡           │
│   ☐ Open tax software              🟢           │
│                                                  │
│  [ ▶ Start Focus ]  [ 🗓 Schedule ]  [ 🗑 ]      │
└────────────────────────────────────────────────┘
```
- "Break it down" turns one scary task into checkable atomic steps (each its own mini-task).
- Schedule = drop onto calendar / pick a time → may create a Nudge.

---

## 5. Inbox / Triage (capture from email, calendar, share)

```
┌────────────────────────────────────────────────┐
│  📥 Inbox — 4 things to triage                   │
│                                                  │
│  ┌──────────────────────────────────────────┐   │
│  │ 📧 "Re: invoice overdue"   from Gmail     │   │
│  │ Suggested: "Pay invoice #1042"            │   │
│  │ [ ✓ Keep ]  [ 🧩 Break down ]  [ 🗓 ]  [🗑]│   │
│  └──────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────┐   │
│  │ 🗓 "Dentist 2:30pm"        from Calendar   │   │
│  │ Suggested prep: "Bring insurance card"    │   │
│  │ [ ✓ Keep ]                          [🗑]  │   │
│  └──────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────┐   │
│  │ 📲 "basket from shared link"  from Share   │   │
│  └──────────────────────────────────────────┘   │
└────────────────────────────────────────────────┘
```
- Each candidate = quick triage: Keep / Break down / Schedule / Trash.
- Sources tagged with an icon so provenance is clear.
- Badge count on the Inbox nav item.

---

## 6. Insights

```
┌────────────────────────────────────────────────┐
│  📊 Insights — last 30 days                      │
│                                                  │
│  Your peak window                                │
│   ▁▂▅█████▅▃▂▁  →  9–11am                        │
│                                                  │
│  Focus sessions:  avg 22 min · 47 total          │
│  Completion rate: 68%                            │
│  Most avoided:    🔴 high-energy tasks (3.1 days)│
│                                                  │
│  Energy vs. done ─────────────────────────────   │
│   🟢 ████████ 41                                 │
│   🟡 █████ 26                                    │
│   🔴 ██ 12                                       │
│                                                  │
│  "You ship most when you start before 11am."     │
└────────────────────────────────────────────────┘
```
- Self-knowledge screen. Charts feed back into the scoring (time-of-day fit).
- Each insight is a plain-language sentence, not just a chart.

---

## 7. Hangfire Dashboard (auth-gated, demo/ops)

- Not user-facing; reachable at `/jobs` behind auth.
- Shows recurring jobs: `recalculate-priority`, `resurface-stale`, `dispatch-nudges`, `sync-integrations`.
- Included because the **screenshot is a great interview artifact** ("here are my background jobs running").

---

## Visual-phase notes (for when Sean is back)
- Mood: **calm + focused**, not gamified-childish. Think Things 3 / Linear calm, with deliberate dopamine accents.
- Energy colors 🟢🟡🔴 are the core visual language — reuse consistently.
- Needs: color palette, type scale, logo/wordmark, the "What Now?" card treatment, focus-mode timer, empty states, dark mode.
- Candidate tools to explore for visuals: Figma mock, or quick HTML/Tailwind static mock of the "What Now?" + Focus screens.
