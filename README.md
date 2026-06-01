# FlowState

> An ADHD-aware task engine that works *with* executive dysfunction, not against it.

FlowState decides **what you should do right now** based on your current energy, and refuses
to overwhelm you with the full list. Built as a full-stack .NET showcase.

**Status:** Planning / pre-Slice 1 · **Stack:** C# / .NET 9 · Blazor Server · PostgreSQL · EF Core · MediatR · SignalR · Hangfire

---

## Why it exists

A normal to-do list shows you forty things and an ADHD brain freezes. FlowState shows you
**one** thing, chosen for the state you're actually in. Avoided tasks decay quietly and
resurface as a decision instead of rotting in a list you never scroll to.

## What it does

- **Brain dump** — capture anything fast, tag energy (low / med / high)
- **"What now?"** — surfaces one task based on energy, time of day, and a priority score
- **Break it down** — turn a scary task into atomic next-actions
- **Focus mode** — a live timer on a single task (SignalR)
- **Energy filtering** — fried? you only see low-effort wins
- **Pick for me** — weighted-random pick to kill choice paralysis
- **Decay & resurfacing** — background jobs keep avoided tasks honest
- **Insights** — learn when and how you actually get things done
- **Capture inbox** — pull candidate tasks from email and calendar

## Documentation

| Doc | What's in it |
|---|---|
| [PROJECT.md](PROJECT.md) | Master plan: purpose, stack, architecture, roadmap |
| [docs/SCREENS.md](docs/SCREENS.md) | Screen specs and wireframes |
| [docs/ALGORITHM.md](docs/ALGORITHM.md) | The PriorityScore + decay engine |
| [docs/SLICE-01.md](docs/SLICE-01.md) | Walking-skeleton build plan |
| [docs/mockup/index.html](docs/mockup/index.html) | Approved visual direction |

## Roadmap (vertical slices)

1. Walking skeleton · 2. Core CRUD + energy + "pick for me" · 3. Decay/resurfacing engine ·
4. Focus sessions + SignalR · 5. Insights · 6. Nudges · 7. Capture inbox ·
8. Email + calendar integrations · 9. PWA · 10. Polish + Azure deploy ·
11. MAUI hybrid · 12. LLM breakdown

---

*Portfolio project by Sean Bilger.*
