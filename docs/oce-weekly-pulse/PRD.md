# PRD — Weekly Pulse (Ripple integration)

> Builds the validated **Weekly Pulse** prototype into Ripple (oceans-app) as a new MVC Area.
> Domain language: [`docs/oce-weekly-pulse/CONTEXT.md`](./CONTEXT.md).
> Decisions of record: [ADR 0001 — Snapshot vs living entities](../adr/0001-snapshot-vs-living-entities.md), [ADR 0002 — Team as a new org entity](../adr/0002-team-as-new-org-entity.md).

## Problem Statement

Oceans runs a weekly operating meeting, but the data for it is scattered and assembled live. Teams arrive without their numbers entered, issues and to-dos live in people's heads or in ad-hoc lists, and the facilitator spends the meeting collecting status instead of running it. There is no shared, week-over-week record: last week's decisions, the trend on a KPI, or whether an issue has been carried for a month are all invisible. Ripple — the company's internal P&C/operations app where people already log in — has no place for any of this today (no Team concept, no KPI concept, no weekly cadence).

From a user's perspective:
- **As a Team Leader**, I show up to the meeting unprepared because there's nowhere to put my team's update *during* the week.
- **As a facilitator**, I waste the meeting gathering numbers and re-explaining old issues instead of driving decisions.
- **As anyone**, I can't see whether we're trending up or down, or what we decided last week.

## Solution

A **Weekly Pulse** area inside Ripple: one set of data per **Week**, viewed in two modes — a **Dashboard** that teams fill in *during* the week, and a **Weekly Pulse Review** that organizes the same data into a guided meeting agenda. Teams enter **check-ins**, **KPI results**, **headlines**, **issues**, and **to-dos** as the week happens; when the meeting starts, everything is already sorted by Team in meeting order, with the right things surfaced (problems loud, healthy stuff quiet) and the rest tucked away.

The model rests on one distinction (ADR 0001): **snapshot entities** (check-ins, KPI results, headlines, weekly summary) belong to exactly one Week and start blank each Week; **living entities** (issues, to-dos) are created once and persist across Weeks until they reach a terminal state. On top of that sit read-only lenses — **Meeting History** (past weeks as minutes) and **KPI History** (one KPI's trend by period) — and an auto-assembled, read-only **Weekly Summary** draft.

From a user's perspective:
- **As a Team Leader**, I keep my team's Dashboard current through the week, so the meeting is a review, not a data-entry session.
- **As a facilitator**, I run the Review top-to-bottom: each team's news, its KPIs that need attention, its open issues and to-dos — already filtered.
- **As anyone**, I can open KPI History to read a trend, or Meeting History to see what we decided three weeks ago.

## User Stories

### Weeks & cadence
1. As a participant, I want the current Week to be determined automatically from the calendar (Monday–Sunday, Costa Rica time), so that I never have to create or "advance" a week by hand.
2. As a participant, I want each new Week to start with blank snapshots (check-ins, KPI results, headlines, summary), so that the Dashboard reflects only this week's input.
3. As a participant, I want open issues and to-dos to carry forward into the new Week automatically, so that unresolved work is never lost or re-typed.
4. As a facilitator, I want the Week to be created lazily the first time anyone touches it, so that empty future weeks don't clutter the system.

### Dashboard (input mode)
5. As a Team Leader, I want to record one **check-in** per week for my team (Win / Concern / Priority / Other + a note), so that the meeting opens with a segue.
6. As a Team Leader, I want re-saving my check-in to overwrite the existing one, so that there's never a duplicate for the same team and week.
7. As an owner, I want to enter a **KPI result** (free-text value, a Green/Yellow/Red status I choose, and notes) for each of my team's KPIs, so that the numbers are ready before the meeting.
8. As an owner, I want re-saving a KPI result to update it in place, so that there's exactly one result per KPI per week.
9. As a Team Leader, I want to post **headlines** for my team — Highlight (a win) or Risk (a concern) — so that the meeting has a good-news/bad-news round.
10. As an owner, I want to raise an **issue** (title, priority, owner) on my team, so that it's queued for discussion.
11. As an owner, I want to add a **to-do** with one owner and a due date, so that an action is tracked to completion.
12. As a participant, I want the Dashboard to show all open issues and all non-done to-dos for a team (a running list), so that nothing falls off until it's resolved.
13. As a participant, I want snapshot inputs (check-in, KPI results, headlines) to appear blank at the start of each week, so that the Dashboard never shows stale carry-over.

### Issues — lifecycle & IDS
14. As an owner, I want to move an issue through Open → Deferred → Solved, so that its state reflects where it is in the IDS process.
15. As a facilitator, I want to **Defer** an issue (consciously park it), so that it stays on the Dashboard but drops out of the meeting until reactivated.
16. As an owner, I want to add IDS comments to an issue over time, so that its discussion history is preserved.
17. As an owner, I want an issue's priority (Low/Med/High/Critical) to be a label only, so that priority never silently hides or surfaces it.
18. As a facilitator, I want a **Solved** issue to leave the Review automatically, so that the meeting only shows live problems.

### To-dos — lifecycle
19. As an owner, I want to move a to-do through Open → Blocked → Done, so that blockers are visible.
20. As a facilitator, I want **Blocked** to-dos to stay loud in the Review, so that the meeting clears them.
21. As a facilitator, I want **Done** to-dos to drop out of the Review, so that finished work doesn't take meeting time.

### Pin to Review
22. As a facilitator, I want to **pin** a Deferred issue back into the Review, so that I can un-park it for this meeting.
23. As a facilitator, I want the pin affordance offered **only on Deferred issues**, so that the control reads as "un-park this" and isn't a confusing no-op on Open issues.
24. As a participant, I do not want headlines to be pinnable, because every headline already surfaces in the news round.

### Weekly Pulse Review (meeting mode)
25. As a facilitator, I want teams shown in a fixed meeting order, so that the meeting follows a predictable agenda.
26. As a facilitator, I want each team's segment to open with its **headlines** as a news round — Risk loud, Highlight quiet — so that we start with good news / bad news.
27. As a facilitator, I want only **in-scope** KPIs to appear in the Review, so that tracked-only metrics don't clutter the meeting.
28. As a facilitator, I want KPIs that are Red, Yellow, or missing to dominate and Green ones to stay quiet, so that attention goes where it's needed.
29. As a facilitator, I want the Review to show the union of **Open** issues and **pinned Deferred** issues (never Solved), so that the discussion list is exactly the live problems plus anything I un-parked.
30. As a facilitator, I want every non-done to-do to surface, with Blocked flagged loud, so that actions and blockers are reviewed.
31. As a facilitator, I want a Review hint explaining what's deliberately hidden (Deferred issues, Done to-dos, out-of-scope KPIs, Green KPIs), so that absence reads as intentional.

### Readiness
32. As a facilitator, I want each team to show a **Readiness** signal (Not configured / Not ready / Ready) based only on whether its in-scope KPIs have results this week, so that I can see at a glance who has reported.
33. As a Team Leader, I want Readiness to mean *reported*, not *healthy* (a Red result still counts as ready), so that the signal measures preparation, not performance.
34. As a participant, I want check-ins, headlines, and issues to not affect Readiness, because they're often legitimately empty.

### Conversions
35. As an owner, I want to convert a **check-in into an issue**, so that a raised concern becomes trackable without re-typing it.
36. As an owner, I want to convert a **headline into an issue** (additively — the headline stays), so that a flagged risk becomes a tracked issue.
37. As an owner, I want to convert an **issue into a to-do**, so that a decision becomes an action.
38. As a participant, I want every conversion to keep a back-reference to its source and to never delete the source, so that provenance is preserved.

### KPI definitions (guarded)
39. As a facilitator, I want to **create** a KPI definition for a team (name, owner, free-text weekly target), so that a new metric is tracked.
40. As a facilitator, I want to **edit** or **retire** a KPI definition, so that the metric set stays current.
41. As a facilitator, I want creating/editing/retiring a KPI definition to require an explicit **confirmation**, so that I feel the difference between a structural change and an everyday edit.
42. As a participant, I want everyday edits (KPI results, issues, to-dos) to have **no** confirmation friction, so that filling in the Dashboard stays fast.
43. As a facilitator, I want to mark a KPI **in scope / out of scope** for the meeting, so that I can track a metric without discussing it.
44. As a facilitator, I want to retire a KPI without losing its history, so that past results stay readable while it stops expecting new input.

### KPI History
45. As a participant, I want a read-only **KPI History** for a single KPI showing its weekly results in sequence, so that I can read the trend by eye.
46. As a participant, I want to group KPI History by **period** (month / quarter / year), so that I can zoom the trend in or out.
47. As a participant, I want each Week assigned wholly to the period containing its Monday, so that a week straddling a boundary isn't split.
48. As a participant, I do **not** want KPI History to sum or average results, because results are free text and judgment-based.

### Meeting History (minutes)
49. As a participant, I want to browse past **Weeks** as meeting minutes, so that I can see what each week contained.
50. As a participant, I want each past Week to show its snapshots plus every living entity that was **active that week**, shown as of that week, so that I can read an issue's progression across the weeks it touched.
51. As a participant, I want a single issue or to-do to appear under each Week it was active in, so that its history is continuous rather than duplicated.

### Weekly Summary
52. As a facilitator, I want the **Weekly Summary** auto-assembled as a read-only draft — decisions from issues Solved this week, actions from to-dos created in the meeting, risks from Risk headlines + High/Critical open issues, and a summary sentence — so that the week's record reads back without my composing it. (Editing/override deferred — see Out of Scope.)

### Settings
53. As a facilitator with Administer rights, I want to manage **Teams** (name, Team Leader, meeting order), so that the roster and agenda order are correct.
54. As a facilitator, I want a Person to be able to lead more than one Team, so that one leader (e.g. David Barrios over Sales and Marketing) is modeled correctly.
55. As a facilitator, I want to pick Owners and Team Leaders from Ripple's existing users, so that I don't re-enter people.

### Access & identity (Ripple integration)
56. As a logged-in Ripple user with the **Participate** permission, I want to view the Weekly Pulse and add/edit any team's everyday data, so that the whole company can keep the operating picture current.
57. As a user with the **Administer** permission, I want exclusive access to structural actions (KPI definitions, deleting Meeting History entries, Settings), so that the model's structure changes deliberately.
58. As a participant, I want Owners to be selectable across teams (an Owner may be on a different Team than the record), so that cross-team accountability is possible.
59. As a participant, I want all grouping (Dashboard, Review, History) to follow the record's **Team**, never the Owner's team, so that a cross-team owner doesn't scatter a team's data.

## Implementation Decisions

### Architecture: pure domain services over EF/HTTP
The logic validated in the throwaway prototype (`docs/oce-weekly-pulse/prototype/model.js`) is ported to **pure C# domain services** with no `DbContext` or `HttpContext` dependency. A controller loads data, calls a domain service, and renders; the service is unit-testable in isolation exactly as the prototype's `model.js` was driven from a shell. This is the central architectural decision (confirmed with the developer). The pure modules:

1. **Week & Period math** — `WeekStart` (the Monday, computed in Costa Rica time, UTC-6 year-round) from an instant; the **Period** bucket (month/quarter/year) containing a `WeekStart`. One authoritative Monday computation reused everywhere (snapshot partitioning, living-entity origin stamping, KPI History grouping). Small shared primitive, not a standalone subsystem.
2. **Review Surfacing** — given a Team's Week data, returns the ordered surfaced set with loud/quiet emphasis. Rules: KPI in-scope gate + Green-quiet; Issues = union of (Open) and (pinned Deferred), Solved excluded; To-Dos = all non-Done, Blocked loud; Headlines = news round, Risk loud, Highlight quiet.
3. **Readiness** — per Team/Week three-state from in-scope KPI definitions vs. results: *Not configured* (no in-scope KPIs), *Not ready* (some in-scope KPI lacks a result this Week), *Ready* (every in-scope KPI has a result with a status).
4. **Living-entity status history** — a week-stamped status/comment **history table** (not an event store) with `stateAsOf(week)` = the latest status row with `week <= w`, plus an `activeInWeek` derivation (active = surfaced in that week's Review OR had a comment/status row that week) driving Meeting History.
5. **KPI History assembly** — groups one KPI's weekly results by Period into the display structure; no arithmetic roll-up.
6. **Weekly Summary derivation** — assembles the **read-only** draft (decisions ← Solved-this-week issues; actions ← in-meeting-created to-dos; risks ← Risk headlines + High/Critical open issues; summary sentence). Computed on the fly from the Week's data — **no persisted `WeeklySummary` entity** until override/editing is needed.

The surfacing rule, ported from the prototype, is precise enough to inline (from `model.js` `reviewSurfacing`):

```
Issue surfaces  ⇔  surfaced && state ≠ Solved
  where surfaced = (state == Open) || pinned
To-Do surfaces  ⇔  state ≠ Done           (Blocked ⇒ loud)
KPI surfaces    ⇔  inScope                 (status ≠ Green ⇒ loud)
Headline surfaces ⇔ always                 (type == Risk ⇒ loud)
```

### Identity & org (from the grilling, see CONTEXT.md + ADR 0002)
- **Person = `ApplicationUser`.** Owner, Team Leader, and a To-Do's owner are `ApplicationUser` references. Pulse does not route through `ConsultantDetail`.
- **Team is a new first-class entity**, owned by the WeeklyPulse area but named generically (`Team`, not `PulseTeam`) so it can graduate later (ADR 0002). v1 shape is **leader-only**: `{ Name, TeamLeader, DisplayOrder }` — no Person↔Team membership table, because nothing in the model reads one.
- **A Person may lead more than one Team** (Team Leader is a many-Teams-to-one-Person relationship).
- **Owner** is a single concept; grouping always follows the record's Team, never the Owner's team.

### Week as a value, not a table
- **Week is a `WeekStart` value** (the CR-time Monday), not its own entity. Every snapshot entity carries a `WeekStart` column; every living entity stamps an `OriginWeekStart`. There is **no `Week` table** — it would currently hold no attributes of its own (the Summary is derived, the meeting rating is out of scope). Promote `Week` to an entity only when it first earns a stored attribute.
- The "current Week" is just `WeekStart(now)` computed from the clock in CR time by the shared Week-math function — there is no row to create and **no manual advance** (the prototype's "Advance week" was a time-machine affordance only). The set of past Weeks for Meeting History is the distinct `WeekStart` values across the data.

### Snapshot vs living (ADR 0001)
- **Snapshot entities** (KPI result, check-in, headline; the Weekly Summary is a derived read-only view, not stored): one per Week (`WeekStart`), blank at the start of each Week, never copied forward.
- **Living entities** (Issue, To-Do): created once, single identity across all Weeks they touch, persist until terminal (Issue → Solved, To-Do → Done). Never copied forward.
- Living-entity **status lives in a week-stamped status/comment history table** (one row per change, not an event store), so `stateAsOf(week)` and Meeting History "as of that week" are derivable rather than stored as denormalized attribution.

### Permissions (Ripple integration)
- A new `WeeklyPulse` `SystemSubArea` with **two coarse policies**: **Participate** (view + everyday edits for any team) and **Administer** (guarded/structural actions: KPI definitions, deleting Meeting History, Settings). Editing is **trust-based, not row-level team-scoped**. "Leadership-the-authority" = the Administer claim, distinct from "Leadership-the-Team" (an ordinary `Team` row).

### Data layer & MVC (existing Ripple patterns)
- **EF entities** in `OceansApp.Models`: `Team`, `KpiDefinition`, `KpiResult`, `CheckIn`, `Headline`, `Issue`, `ToDo`, `LivingEntityEvent` (status/comment history). No `Week` table (it's a `WeekStart` column) and no `WeeklySummary` table (derived read-only). KPI definition carries independent `Active` and `InScope` flags; KPI result carries free-text value + manual Green/Yellow/Red status + notes.
- **Repositories + `IUnitOfWork`** additions in `OceansApp.DataAccess`, following the existing repository/Unit-of-Work pattern; one EF migration; `DbInitializer` seed for an initial Team set and KPI definitions.
- **WeeklyPulse MVC Area** in `OceansAppWeb`: controllers + Razor views + vanilla JS for Dashboard, Review, Meeting History, KPI History, Summary, and Settings — mirroring the prototype's `index.html` views over the same logic core.
- **Guarded mutation** is a client-confirm before the KPI-definition create/edit/retire POST; everyday edits have no confirm.

### Conversions
- Additive only: check-in → Issue, headline → Issue, Issue → To-Do. The new living entity is pre-filled from the source and stores an origin reference (origin type + id); the source is never consumed or deleted.

## Testing Decisions

- **What makes a good test here:** assert external behavior of the pure domain services — given input data (teams, weeks, KPI defs/results, issues with event logs, headlines), assert the decision returned (what surfaces and whether loud/quiet, the Readiness state, the period grouping, the assembled summary). Do not assert internal structure, private helpers, or EF/HTTP wiring. The prototype's `NOTES.md` "try this" scenarios are the behavioral oracle to port.
- **Modules under test (five pure domain services):** Review Surfacing, Readiness, Living-entity status history (`stateAsOf` / `activeInWeek`), KPI History assembly, Weekly Summary derivation. **Week & Period math** is a shared primitive exercised through these (the boundary cases — a week straddling a month, the CR Monday — are asserted via KPI History and surfacing inputs), not a standalone suite.
- **Representative cases to cover** (ported from prototype validation): a pinned Deferred issue surfaces while an unpinned Deferred issue stays hidden; a Solved issue never surfaces; Green KPI quiet, Red/missing loud; out-of-scope KPI never appears; Blocked to-do loud; Readiness flips Not ready → Ready when the last missing in-scope KPI gets a result; a week straddling a month boundary groups by its Monday; an issue active in two weeks appears under both in Meeting History with its as-of state.
- **Prior art:** none — the solution has no test project today (only `appsettings.Test.json`). This work **stands up the first test project** (xUnit) covering the WeeklyPulse domain services. Because the services are pure (no `DbContext`/`HttpContext`), tests need no database or web host — plain arrange/act/assert over in-memory data, exactly like the prototype's `node -e` checks.

## Out of Scope

- **Numeric KPI roll-ups** — KPI History groups and displays free-text results by period; it does not sum, average, or compute totals. Revisit only if KPI results gain a structured numeric type.
- **Row-level / per-team edit permissions** — v1 is trust-based; "a team edits only its own data" enforcement is deferred (would require Person↔Team membership and conflicts with cross-team Owners).
- **Person↔Team membership roster** — not modeled in v1; Owners/leaders are picked from the full `ApplicationUser` directory. Add membership only when a feature consumes it.
- **Person management** — adding/removing users lives in Ripple's existing AdminCenter user admin, not in Pulse Settings.
- **Auto-reactivation of Deferred issues** on a chosen revisit Week — manual reactivation only for now.
- **Meeting rating** — listed as a snapshot in the glossary but not built in v1.
- **Weekly Summary editing / override** — v1 shows the derived draft **read-only** (what the prototype validated). Editing any field, and the `WeeklySummary` table that override would require, are deferred until a facilitator actually needs to override.
- **AI-assisted summary** — the Weekly Summary draft is plain derivation; no LLM.
- **Team nesting / rollups / aggregation** — Teams are flat; nesting is display-only grouping, deferred.

## Further Notes

- The throwaway prototype (`docs/oce-weekly-pulse/prototype/`, browser + TUI over one `model.js`) validated the entire model end-to-end and is the behavioral reference. It should be deleted once this is built (per its own `NOTES.md`).
- The CR-time week boundary is new to Ripple (its native cadence is biweekly pay periods). UTC-6 is year-round (no DST), which simplifies the Monday computation — but every call site must use the one shared `WeekStart` function, never ad-hoc date math.
- This PRD is built in **three gated phases** — see **Phases & rollout** below.

## Phases & rollout

The work is sliced into **three tracer-bullet phases**, each a complete, independently-testable path through every layer. Each phase is a GitHub **milestone** holding a tracking **epic** plus thin vertical-slice issues; an agent loop runs the slices of one phase at a time.

**The phases are gated: the sandcastle build loop stops at each phase boundary.** Sandcastle works through a backlog file (`scripts/ralph/prd.<...>.json`), running the lowest-`priority` unfinished story until none remain — so the gate is **one backlog file per phase**:

- `scripts/ralph/prd.weekly-pulse.phase1.json` — the 4 Phase 1 slices (#27–#30)
- `scripts/ralph/prd.weekly-pulse.phase2.json` — the 6 Phase 2 slices (#31–#36)
- `scripts/ralph/prd.weekly-pulse.phase3.json` — the 3 Phase 3 slices (#37–#39)

All three target one shared branch (`sandcastle/weekly-pulse`); each story's `notes` reference its `GitHub issue #N`, which sandcastle closes on success. Run a phase with `SANDCASTLE_PRD=scripts/ralph/prd.weekly-pulse.phase1.json npm run sandcastle`; the loop builds those slices and **stops** (no more unfinished stories). Verify, then run the next phase's file. The GitHub `ready-for-agent` / `blocked` labels are a parallel human/tracker signal of the same gating — they don't drive sandcastle, the backlog files do.

| Phase | Milestone / epic | Scope | Gate |
|---|---|---|---|
| **1 — Spine** | epic #24 · slices #27–#30 | Area + auth + `Team` + `WeekStart`; one snapshot (Check-in) + one living entity (Issue) end-to-end; pin + issue surfacing; first xUnit project | **ready now** (`ready-for-agent`) |
| **2 — Runnable meeting** | epic #25 · slices #31–#36 | KPIs (guarded) + Readiness, Headlines, To-Dos, Conversions, full surfacing, Settings | **held** (`blocked`) until Phase 1 done |
| **3 — Lenses** | epic #26 · slices #37–#39 | Meeting History (`activeInWeek`), KPI History by Period, derived read-only Weekly Summary | **held** (`blocked`) until Phase 2 done |

Within a phase, slices still have `Blocked by` ordering (e.g. the walking skeleton #27 unblocks the rest of Phase 1); the loop should take the unblocked slice first.
