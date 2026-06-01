# Slice 1 — Walking Skeleton

> Goal: prove the whole architecture end-to-end with the smallest possible vertical slice,
> and get CI green. No features yet — just a thin line through every layer.

**Definition of done:** From a clean clone, `docker compose up` (or `dotnet run`) shows a
Blazor page listing tasks from PostgreSQL, and a pushed branch gets a green GitHub Actions run.

---

## Scope (in)
- Solution + project skeleton per architecture (Domain, Application, Infrastructure, Api, Web, Workers + test projects). Integrations/Mobile stubbed/omitted until their slices.
- One entity end-to-end: **TaskItem** (Id, Title, EnergyCost, Status, CreatedAt).
- EF Core + Npgsql + one migration; Postgres via docker-compose.
- One Application query (`GetTasks`) via MediatR.
- Blazor Server page that lists tasks + a trivial add box.
- Serilog + a `/health` endpoint + Swagger on the API.
- xUnit: one unit test (Domain) + one integration test (Testcontainers Postgres) hitting `GetTasks`.
- GitHub Actions workflow: restore → build → test, green badge.

## Scope (out — later slices)
- Energy filtering / scoring (Slice 2–3), SignalR (4), insights (5), nudges (6),
  capture/triage (7), integrations (8), PWA (9), auth (10), MAUI (11), LLM (12).

---

## Step-by-step

1. **Solution scaffold**
   - `dotnet new sln -n FlowState`
   - Create `src/` projects: `classlib` Domain, `classlib` Application, `classlib` Infrastructure,
     `webapi` Api, `blazor` (Server) Web, `worker` Workers.
   - Create `tests/` projects: `xunit` UnitTests, `xunit` IntegrationTests.
   - Wire project references per the dependency rule; add all to the sln.

2. **Domain** — `TaskItem` entity + `EnergyCost` enum (Low/Med/High) + `TaskStatus` enum.

3. **Application** — MediatR; `GetTasksQuery` + handler; `ITaskRepository` interface.

4. **Infrastructure** — `FlowStateDbContext` (Npgsql), `TaskItem` config, `TaskRepository`,
   DI registration; initial EF migration.

5. **Api** — minimal API `GET /tasks` (calls MediatR), Swagger, Serilog, `/health`.

6. **Web (Blazor Server)** — `Tasks.razor` lists tasks (via Application/MediatR directly,
   since Blazor Server can call the app layer in-process), plus a minimal add box.

7. **docker-compose** — `postgres` service + the web/api; `.env.example` for connection string.

8. **Tests**
   - Unit: a Domain invariant (e.g., new TaskItem defaults to Open + has CreatedAt).
   - Integration: Testcontainers Postgres → migrate → seed → assert `GetTasks` returns them.

9. **CI** — `.github/workflows/ci.yml`: setup .NET 9 → restore → build → `dotnet test`.

10. **README v0** — one-paragraph what/why + "how to run locally" + CI badge placeholder.

---

## Acceptance checklist
- [ ] `dotnet build` clean across the solution
- [ ] `dotnet test` passes (1 unit + 1 integration) locally
- [ ] `docker compose up` → Blazor page lists seeded tasks from Postgres
- [ ] GitHub Actions run is green on the feature branch
- [ ] Branch opened as PR for Sean's review (no push to main)

---

## Notes / decisions to confirm before coding
- Repo on GitHub: name (`flowstate`?), public vs private (public = portfolio visibility).
- Rename default branch `master` → `main` before first push.
- .NET 9 SDK confirmed installed on this machine (check at kickoff: `dotnet --version`).
- Blazor Server calling MediatR in-process vs going through the Api over HTTP — Slice 1 uses
  in-process for simplicity; revisit if/when WASM or MAUI needs the HTTP API contract.
