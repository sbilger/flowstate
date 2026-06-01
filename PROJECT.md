# FlowState — Project Plan

> An ADHD-aware task engine that works *with* executive dysfunction, not against it.
> Portfolio flagship project demonstrating full-stack .NET breadth.

**Status:** Planning
**Owner:** Sean Bilger
**Last updated:** 2026-05-31

---

## 1. Purpose

Two goals, in priority order:

1. **Job-hunt showpiece.** A polished, deployed, well-documented project that demonstrates
   breadth across the modern .NET stack to potential employers. Linked from seanbilger.xyz.
2. **A tool I actually use.** Built around how ADHD brains work, so the passion is genuine
   and the interview narrative ("I built the tool I wished existed") is real.

### Why this project (interview framing)
It is deliberately *not* another to-do app. The ADHD-aware mechanics each justify a distinct
piece of real engineering, so the tech breadth looks intentional rather than bolted on.

---

## 2. What the app actually does (user journey)

**One-liner:** FlowState decides *what you should do right now* based on your current energy,
and refuses to overwhelm you with the full list.

- **Brain dump.** Throw in everything; quick energy tag 🟢 Low / 🟡 Med / 🔴 High (manual or guessed).
- **Break it down.** Scary task → atomic next-actions (rule-based first, LLM later).
- **"What now?"** Surfaces ONE task based on time-of-day, current energy, and priority score.
- **Focus mode.** Live timer (SignalR) on a single task; log the outcome; feeds stats.
- **Energy filtering.** Fried? Only low-cost tasks are shown — never taunted with work you can't do.
- **"Pick for me."** Weighted random pull to kill choice paralysis.
- **Decay & resurfacing.** Avoided tasks quietly decay, then resurface reframed so nothing rots.
- **Gentle nudges.** One well-timed notification, not spam.
- **Insights.** "You finish most tasks 9–11am"; session length averages; avoidance patterns.

### Screens (Blazor)
1. Today / "What now?" (hero) · 2. Brain dump / All tasks · 3. Task detail (+break down) ·
4. Focus mode · 5. Insights · 6. Triage (capture inbox) · 7. Hangfire dashboard (auth-gated, demo)

---

## 3. Core differentiators → engineering mapping

| Feature | What it does | Tech it justifies |
|---|---|---|
| **Atomic breakdown** | Big scary task → tiny next-actions (rule-based first, LLM later) | Service layer logic, later Semantic Kernel |
| **Task decay & resurfacing** | Ignored tasks lose priority, then resurface so they don't rot | Hangfire recurring job + scoring algorithm |
| **Energy-based filtering** | "I'm fried" → only show low-effort tasks | Tagging + query filters |
| **"Pick for me" randomizer** | Removes choice paralysis; dopamine-friendly | Weighted random selection |
| **Focus sessions** | Start a timer on one task, log the session | SignalR live timer + session history |
| **Gentle nudges** | One well-timed notification, not spam | Hangfire scheduled jobs + notification service |
| **Insights dashboard** | "You finish most tasks 9–11am" | Time-series aggregation + caching + charts |
| **Capture inbox** | Tasks flow in from email, calendar, share-to-app | OAuth, webhooks, sync jobs, ingestion abstraction |

**The algorithmic heart:** `PriorityScore` + `DecayState`. Recency, importance, age,
snooze count, energy match, and time-of-day fit combine into a score the resurfacing worker
recalculates on a schedule. This is the "interesting problem I solved" story.

---

## 4. Integrations & Capture Inbox

**Concept:** one ingestion pipeline turns anything into a *candidate task* you triage
(keep / break-down / schedule / trash). Architecturally: an `Integrations` module with an
`IIngestionSource` abstraction + per-provider adapters + OAuth token storage + sync jobs.

| Source | Status | Mechanism | Feeds |
|---|---|---|---|
| 📧 **Email** | Core | Gmail API + Microsoft Graph (OAuth); webhook/push or Hangfire poll | Candidate tasks |
| 📅 **Calendar** | Core | Google Calendar API + Microsoft Graph (OAuth) | Free/busy → timing & nudges; events → prep tasks |
| 📲 **Share-to-app** | Core | PWA / MAUI share target (send from any app incl. Messages) | Candidate tasks |
| 💬 **Text (Twilio)** | Deferred | Dedicated number; inbound SMS webhook | Candidate tasks — revisit after core ships |
| 💬 **iMessage** | Out of scope | No legitimate API (chat.db is Mac-only/ToS-adjacent) | Not planned |

**Decision (2026-05-31):** messaging (Twilio/iMessage) is **skipped for now**. The "send a
task from anywhere, including Messages" goal is still met via the **share-to-app** target.
Email + Calendar are the headline integrations. Twilio can be added later as a clean,
self-contained slice if desired.

**How integrations feed the brain:**
- Calendar free/busy → "What now?" respects your next meeting; nudges land in real gaps.
- Calendar events → auto-spawn prep tasks.
- Email → candidate tasks landing in Triage.

---

## 5. Platform / mobile

Build order: **Web (Blazor Server) → PWA → MAUI Blazor Hybrid (later slice).**

| Stage | What you get | .NET story |
|---|---|---|
| Web (Blazor Server) | Desktop/laptop app | Core |
| PWA | Installable, home-screen, push, share target, offline-ish | "Mobile capture, cheap" |
| MAUI Blazor Hybrid | Native iOS/Android **reusing the exact Razor components**, native share + notifications | "One C# codebase, web + native" — killer line |

---

## 6. Cost (run it for ~$0–15/mo)

**Principle:** free tiers + local models by default; every paid dependency optional/swappable.

| Piece | Cost | Notes |
|---|---|---|
| Gmail / Google Calendar API | Free | Quotas far exceed personal use |
| Microsoft Graph | Free | |
| Inbox email domain | $0 | Use a subdomain on a domain you already own |
| Database (Postgres) | $0 | Neon or Supabase free tier (not paid Azure Postgres ~$12+/mo) |
| Hosting | $0–13/mo | Azure App Service F1 free, or Railway/Render free; ~$13 B1 only for always-on demo |
| LLM (later slice) | ~$0 | Local model via Ollama on RTX 3070 Ti, or OpenAI at pennies |
| MAUI native | $0 build/run | Apple Developer $99/yr only for physical iPhone / App Store |
| Twilio (deferred) | ~$1.15/mo + ~$0.008/text | Only if messaging slice is added later |

**Bottom line:** development effectively free; polished always-on interview demo ~$0–15/mo.

---

## 7. Tech stack (locked)

| Concern | Choice | Notes |
|---|---|---|
| Language / runtime | C# / .NET 9 | |
| Frontend | **Blazor Server** | + PWA, then MAUI Blazor Hybrid later |
| API | ASP.NET Core Web API + minimal APIs | |
| Database | **PostgreSQL** | Neon/Supabase free tier |
| ORM | EF Core + migrations | Npgsql provider |
| App patterns | CQRS via MediatR + FluentValidation | |
| Real-time | SignalR | Focus timer + live task updates |
| Background jobs | **Hangfire** | Dashboard = great demo screenshot |
| Integrations | Gmail API, Microsoft Graph, Google Calendar | OAuth + webhooks + sync jobs (Twilio deferred) |
| Auth | ASP.NET Core Identity | Single-user first, multi-capable |
| Caching | IMemoryCache (→ Redis if needed) | |
| AI (later slice) | Semantic Kernel (local via Ollama) | Optional LLM task breakdown |
| Logging | Serilog (structured) | + health checks |
| API docs | Swagger / OpenAPI | |
| Tests | xUnit + Testcontainers | Unit + integration |
| CI/CD | GitHub Actions | Build + test, then deploy |
| Containerization | Docker + docker-compose | API + Postgres + (Redis) |
| Hosting | **Azure App Service (F1) + Neon Postgres** | Decided 2026-05-31; deploy ~Slice 10. Subdomain flowstate.seanbilger.xyz via CNAME |

---

## 8. Architecture (Clean Architecture)

```
FlowState.sln
├── src/
│   ├── FlowState.Domain          → entities, value objects, domain logic (no deps)
│   ├── FlowState.Application     → CQRS handlers (MediatR), interfaces, validators
│   ├── FlowState.Infrastructure  → EF Core, repositories, notifications
│   ├── FlowState.Integrations    → IIngestionSource adapters (email, calendar), OAuth
│   ├── FlowState.Api             → ASP.NET Core Web API
│   ├── FlowState.Web             → Blazor Server frontend (+ PWA)
│   ├── FlowState.Mobile          → MAUI Blazor Hybrid (later slice)
│   └── FlowState.Workers         → Hangfire jobs (decay, resurfacing, nudges, sync)
└── tests/
    ├── FlowState.UnitTests
    └── FlowState.IntegrationTests
```

**Dependency rule:** Domain depends on nothing. Application depends on Domain. Everything
else depends inward. Interfaces in Application, implementations in Infrastructure/Integrations.

---

## 9. Data model (first cut)

- **User** → Tasks, FocusSessions, EnergyLogs, IntegrationAccounts
- **TaskItem** → Title, Notes, EnergyCost (Low/Med/High), Status, PriorityScore, DecayState,
  SnoozeCount, ParentTaskId (subtasks), Tags, Source, CreatedAt, UpdatedAt, CompletedAt
- **CandidateTask** → raw captured item awaiting triage (Source, Payload, ReceivedAt)
- **IntegrationAccount** → Provider, OAuth tokens, sync cursor, status
- **FocusSession** → TaskId, StartedAt, EndedAt, Outcome
- **EnergyLog** → UserId, Timestamp, Level
- **Nudge** → TaskId, ScheduledFor, Delivered, DeliveredAt
- **Tag** → Name (many-to-many with TaskItem)

---

## 10. Build roadmap (vertical slices)

Each slice is independently shippable and keeps CI green.

1. **Walking skeleton.** Solution structure, EF Core + TaskItem, Blazor page, CI green.
2. **Core CRUD** + energy tagging + "pick for me" randomizer.
3. **Decay/resurfacing.** Hangfire worker + PriorityScore algorithm. *(The meat.)*
4. **Focus sessions** + SignalR live timer.
5. **Insights dashboard** + charts + caching.
6. **Nudges** + notification service.
7. **Capture inbox.** Triage screen + ingestion abstraction + share-to-app target.
8. **Email + Calendar integrations** (Gmail/Graph OAuth, calendar free/busy + prep tasks).
9. **PWA** (installable, push, share target).
10. **Polish.** Auth, Docker, hosting deploy, README with diagram + screenshots, demo seeder.
11. **MAUI Blazor Hybrid** native app (component reuse, native share/notifications).
12. **(Stretch) LLM breakdown** via Semantic Kernel (local Ollama).
13. **(Deferred) Messaging slice** — Twilio inbound SMS, if/when wanted.

---

## 11. Definition of done (portfolio-ready)

- [ ] Deployed live demo with seeded data
- [ ] README: what/why, architecture diagram, screenshots, run instructions, tech list
- [ ] Clean, readable commit history (conventional commits)
- [ ] Green CI badge (build + tests)
- [ ] Meaningful test coverage on the scoring algorithm + core use cases
- [ ] Hangfire dashboard reachable (auth-gated) for the demo
- [ ] At least one live integration demoable (email or calendar)
- [ ] Linked from seanbilger.xyz with a short writeup

---

## 12. Open questions / decisions deferred

- ~~Hosting~~ DECIDED 2026-05-31: Azure App Service (F1 free) + Neon Postgres (free). Viewable at flowstate.seanbilger.xyz (CNAME → Azure; does not touch the Spaceship site). Deploy at Slice 10. Railway = fallback if Azure setup is painful.
- Redis: only introduce if IMemoryCache proves insufficient.
- Email ingestion: push/webhook vs Hangfire polling — decide at Slice 8.
- Apple Developer account ($99/yr): only if shipping MAUI to a physical iPhone.
- Messaging (Twilio): deferred; revisit after core + email/calendar ship.

---

## 13. Working rules

- All work on feature branches → PR for Sean's review. No pushes to `main` until reviewed.
- Back up local work before pulling.
- No "verified/passing" claims until a check actually passes.
- Cost discipline: free tiers + local models by default; paid deps optional and swappable.
