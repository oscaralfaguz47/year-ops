# OCE Weekly Pulse

A weekly operating dashboard for Oceans Code Experts. Teams add updates *during the week* so that when the Weekly Pulse meeting starts, everything is already organized and ready to review. One set of data, viewed in two modes (Dashboard for input, Weekly Pulse Review for the meeting).

## Language

**Week**:
The Monday–Sunday operating period that organizes all activity, with boundaries in **Costa Rica time (UTC-6, year-round — no DST)**. The current Week is created/loaded lazily on first access after the Monday 00:00 CR rollover. Acts as a hard partition for snapshot data and as an origin stamp for living data. In Ripple a Week is a **`WeekStart` value** (the Monday date, computed in CR time), **not its own table**: every **Snapshot entity** carries a `WeekStart` column and every **Living entity** stamps an `OriginWeekStart`. The "current Week" is `WeekStart(now)` — there is no row to create and no manual advance. Ripple has no prior week concept — its native cadence is biweekly pay periods — so the CR-time Monday boundary is new and must be computed consistently everywhere by one shared function (KPI History's **Period** grouping reuses the same authoritative Monday). Promote `Week` to a persisted entity only when it first earns an attribute of its own.
_Avoid_: "Meeting record", "L10", "session"; deriving week boundaries ad hoc at each call site; biweekly/pay-period cadence

**Snapshot entity**:
A record that belongs to exactly one Week and is genuinely blank at the start of each new Week. The stored snapshots are KPI results, check-ins, and headlines/highlights. (The **Weekly Summary** is a *derived* read-only view over a Week's data, not a stored snapshot in v1; the meeting rating is deferred.)

**Living entity**:
A record created *in* a Week (stamped with its origin Week) that remains active and resurfaces in every subsequent Weekly Pulse Review until it reaches a terminal state. It is never re-created or copied forward — it persists. Issues and To-Dos are the living entities.
_Avoid_: copying/cloning a living entity into a new week

**Issue**:
A **Living entity** — a problem needing discussion, alignment, a decision, or action, worked through the IDS framework (Identify, Discuss, Solve). Terminal state: Solved. Surfaces on both the Dashboard (running open list) and the Review (filtered) until solved.

**To-Do**:
A **Living entity** — a single action with exactly one owner and a due date. Terminal state: Done. Surfaces on both the Dashboard and the Review until done.
_Avoid_: "task" as a separate concept, "action item"

**Deferred** (Issue state):
Consciously parked. The Issue stays on the Dashboard's open list but is excluded from default Review surfacing until reactivated (optionally auto-reactivated on a chosen revisit Week). Deferred goes *quiet*.

**Blocked** (To-Do state):
Cannot proceed. The To-Do keeps surfacing in the Review — being blocked is what the meeting needs to clear. Blocked stays *loud*. Non-terminal.

**Check-in** (Segue):
A **Snapshot entity** — exactly **one per (Team, Week)**, owned by the Team Leader, with a single type (Win / Concern / Priority / Other) and a note. Upsert: re-saving overwrites; never duplicated. The Segue shows one card per Team in team order. Can be converted into an Issue.

**Headline**:
A **Snapshot entity** — a short piece of weekly news for a Team, of one type: **Highlight** (a win to celebrate) or **Risk** (a concern to flag). Belongs to exactly one Week and is blank at the start of the next. In the **Weekly Pulse Review** the team's headlines open its segment as a quick **news round**: *all* of the week's headlines surface (it's the meeting's good-news/bad-news round, EOS-style), with **Risk** flagged *loud* and **Highlight** kept *quiet*. Can be converted into an Issue (additive — the headline is preserved). Feeds the Weekly Summary's risks (Risk-type only).
_Avoid_: treating a headline as living — it does not carry forward

**Person**:
An individual who can own or lead Weekly Pulse records. In Ripple, a Person **is an `ApplicationUser`** (the login identity — name, email, photo, auth), referenced by its user id. **Owner**, **Team Leader**, and a To-Do's owner are all `ApplicationUser` references. Pulse deliberately does **not** route through `ConsultantDetail` (the HR/employment record): Pulse needs identity, not payroll/PTO data, and anchoring on `ConsultantDetail` would wrongly exclude any login user (e.g. a Leadership facilitator) who is not a billable consultant.
_Avoid_: keying a Person on `ConsultantId`

**Team**:
A functional org unit (e.g. Sales, Marketing, Operations, Leadership) grouping Persons, with exactly one **Team Leader**. Owns its own KPIs, check-ins, headlines, issues, and to-dos. Order in the roster is also the meeting order. Teams are flat; nesting is display-only grouping for now (no rollup, aggregation, or inheritance). In Ripple a Team is a **new first-class entity** — Ripple has no pre-existing org-unit concept (only delivery-oriented `Project`/`Client`, which Pulse does **not** derive Teams from). It is **owned by the WeeklyPulse area** but modeled as a generic `Team` (not `PulseTeam`) so it can later graduate into a shared, company-wide org concept. See [ADR 0002](../adr/0002-team-as-new-org-entity.md).

In v1 a Team is **leader-only**: `{ Name, Team Leader (`ApplicationUser`), display order }` — there is **no Person↔Team membership roster**, because nothing in the model reads one (Owners may be cross-team and are picked from the full `ApplicationUser` directory; meeting order is *Team* order; Readiness is KPI-only). "Grouping Persons" is conceptual, not a stored member list. Add membership later only when a feature actually consumes it (e.g. a "who's on this Team" view).
_Avoid_: deriving a Team from `Project`/`ProjectConsultantAssigned`; naming it `PulseTeam`; modeling a member roster nothing reads

**Team Leader**:
The one Person who leads a Team. A Team has exactly one Leader, but a Person may lead **more than one Team** (e.g. David Barrios leads both Sales and Marketing).

**Owner**:
The Person accountable for a specific record — e.g., the owner of a KPI, an Issue, or a To-Do. Defaults to the Team Leader but can differ, and may be a Person from a **different Team** than the record's `Team`. Grouping everywhere (Dashboard, Review, History) follows the record's **Team**, never the Owner's team. This is a single concept; do not split it into "owner/leader" on records.

**KPI definition**:
A recurring KPI set up once per Team (name, owner, weekly target as **free text** — e.g. "≥ 95%", "< 2 days", "$50k"). Configured in Settings. Minimum 2 per Team. Carries two independent flags: **active** (live vs retired) and **in meeting scope** (see below). A KPI definition is **structural and stable** — it is *not* an everyday edit like a KPI result or a living entity.

**Guarded mutation** (KPI definition):
Creating, editing, or retiring a **KPI definition** is **guarded** — it requires an explicit confirmation ("you're about to create/change a KPI…"). The guard is deliberate friction that signals KPI definitions are **structural**, changed rarely, *unlike* the frictionless everyday edits: a **KPI result** (the weekly value), **Issues**, and **To-Dos** all change freely with no confirm. The guard reinforces the mental model, not a permission check (there are no permissions in the mockup).

**Active** (KPI flag):
Whether a KPI definition is **live** or **retired**. A retired (inactive) KPI stops expecting weekly results — it no longer prompts for input, doesn't count toward **Readiness**, and drops out of the Review — but its historical KPI results stay intact. Distinct from **In meeting scope**: a KPI can be live-but-out-of-scope (tracked, not discussed) or retired (not tracked at all).

**KPI result**:
A **Snapshot entity** — one Team's actual result (**free text**) for one KPI in one Week, with a manually chosen status (Green / Yellow / Red) and notes. No arithmetic comparison to target — status is judgment. Exactly one result per (KPI, Week): if it exists it is updated, never duplicated.

**In meeting scope** (KPI flag):
The "Include in Weekly Pulse Review" flag on a **KPI definition** — a *scope gate*. A KPI out of scope never appears in the Review regardless of status. Among in-scope KPIs, the surfacing rules decide emphasis (Red/Yellow/missing dominate; Green stays quiet).

**Pin to Review** (Issue flag):
The "Include in Weekly Pulse Review" flag on an **Issue** — an *additive pin*. Issue surfacing is **state-only**: an **Open** issue auto-surfaces (any priority — priority never gates surfacing, it is only a label), **Solved** never surfaces, and **Deferred** goes quiet. The pin's job is to **override the quiet** — pulling a **Deferred** issue back into the meeting. The Review shows the **union** of (Open issues) and (pinned Deferred issues). Because pinning only does real work on a Deferred issue (Open ones surface anyway, Solved ones are done), the pin affordance is **offered only on Deferred issues** — it reads as *"un-park this for the meeting."* **Headlines are not pinnable**: every headline already surfaces in the Review's news round (see **Headline**), so there is nothing to pin.
_Avoid_: "Low-priority" gating; pinning headlines; offering pin on Open/Solved issues

**Conversion**:
Turning one record into a new living entity, always **additive**: a check-in or headline → a new Issue; an Issue → a new To-Do. The new record is pre-filled from the source and holds a back-reference to its origin (origin type + id). The **source is always preserved** — conversions never consume or delete it. Snapshot sources (check-ins, headlines) stay intact in their Week.
_Avoid_: "convert" implying the source disappears

**Meeting History**:
The browsable record of past Weeks, read as **meeting minutes**: each Week shows its snapshot entities plus every **living entity that was active that week** — surfaced in that week's Review or given an IDS comment / status change that week — shown as of that week. A single Issue or To-Do may therefore appear under several Weeks, showing its progression. Entries can be deleted.

**KPI History**:
A read-only lens on a **single KPI** across time, grouping its weekly **KPI results** by **period** (month / quarter / year). Shows each Week's free-text result and status chip in sequence — a trend read *by eye*, not a computed number. Period grouping is **display only**: there is **no arithmetic roll-up** of values (they are free text — see KPI result). Distinct from **Meeting History**, which is the week-by-week minutes across all entities; KPI History follows one KPI down its column.
_Avoid_: summing/averaging KPI results, "quarterly total"

**Period** (KPI History grouping):
A calendar month, quarter, or year used to group a KPI's weekly results in **KPI History**. A **Week** is assigned to the period containing its **start (Monday, CR time)**, so a week that straddles a month/quarter boundary belongs wholly to the period of its Monday — no splitting a Week across periods.

**Active in a Week** (living entity):
A living entity is "active in" a Week if it was surfaced in that week's Review or received a comment / status change that week. Determines which Weeks it appears under in Meeting History. Derivable from comment/status timestamps — not a stored attribution.

**Weekly Summary**:
A Week's meeting record, presented as an **auto-assembled, read-only draft** computed from the Week's data — *not* a stored entity in v1: main decisions ← Issues Solved this Week; main actions ← To-Dos created in-meeting this Week; main risks ← Risk-type Headlines + High/Critical open Issues; summary text ← the suggested-format sentence. Plain derivation, no AI. **Editing/override any field is deferred** (it would require persisting a `WeeklySummary` record) until a facilitator actually needs to correct the draft — the prototype validated only the derivation, read-only.
_Avoid_: treating the Summary as a stored snapshot or a blank form to fill in (v1)

**Readiness**:
A per-Team, per-Week signal with three states, based **only** on KPIs:
- **Not configured** — the Team has no in-meeting-scope KPI definitions (a setup gap; never counts as ready).
- **Not ready** — some in-scope KPIs lack a KPI result for the current Week.
- **Ready** — every in-scope KPI has a KPI result this Week with a status selected (Red counts; readiness means *reported*, not *healthy*).
Check-ins, headlines, and issues do not affect readiness — they are often legitimately empty.

## Scope notes

**No Team "Active" flag.** The team-level active flag (§7) is removed — with a freely-editable roster and no permissions, an inactive team is just one you delete or never add. (The KPI-level **Active** flag is unrelated and kept.)

**Settings scope.** Pulse **Settings** manages **Teams** (name, Team Leader, order) and **KPI definitions** (the **Guarded mutation** UX — a live design question, hence in the mockup). It does **not** manage **Persons**: in Ripple, Persons are existing `ApplicationUser`s administered by the AdminCenter user admin, so Pulse just references them. Person↔Team membership is not modeled (see **Team**). (In the mockup, Person management was out of scope as well-understood CRUD that probes none of the model questions.)

**No numeric KPI roll-ups (yet).** KPI History groups and displays weekly results by period — it does **not** sum, average, or otherwise compute over KPI result values, which are free text. Numeric/quarterly aggregation is **out of scope for now** (noted for Andrés): the mockup validates the *history lens*, not computed totals. Revisit only if KPI results gain a structured numeric type.

**Permissions (Ripple integration).** The mockup is fully open. Inside Ripple, access is gated through Ripple's **claims/policy** pattern (a new `WeeklyPulse` `SystemSubArea`), at **coarse granularity**: two policies — **Participate** (view + everyday edits: check-ins, KPI results, headlines, issues, to-dos for any team) and **Administer** (the guarded/structural actions: KPI definition create/edit/retire, deleting Meeting History, Settings/Person management). Editing is **trust-based, not row-level team-scoped** — a Participant may edit any Team's data. Row-level "a team edits only its own data" enforcement is intentionally **not** built: it would require a load-bearing Person↔Team membership (deferred with Settings/Person management) and conflicts with the cross-team **Owner**. Revisit only if abuse appears.

## Flagged ambiguities

**"Leadership"** — used in the spec both as Team #1 (Eder's team, which reports like any other) and as a permission tier ("Leadership can edit everything"). On Ripple integration these **cleanly separate**: **Leadership-the-Team** is an ordinary `Team` row that reports like any other; **Leadership-the-authority** is the **Administer** policy/claim and is *not* tied to membership in the Leadership Team. (Resolved — no longer ambiguous.)
