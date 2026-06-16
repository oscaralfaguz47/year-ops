# OCE Weekly Pulse

A weekly operating dashboard for Oceans Code Experts. Teams add updates *during the week* so that when the Weekly Pulse meeting starts, everything is already organized and ready to review. One set of data, viewed in two modes (Dashboard for input, Weekly Pulse Review for the meeting).

## Language

**Week**:
The Monday–Sunday operating period that organizes all activity, with boundaries in **Costa Rica time (UTC-6)**. The current Week is created/loaded lazily on first access after the Monday 00:00 CR rollover. Acts as a hard partition for snapshot data and as an origin stamp for living data.
_Avoid_: "Meeting record", "L10", "session"

**Snapshot entity**:
A record that belongs to exactly one Week and is genuinely blank at the start of each new Week. Includes KPI results, check-ins, headlines/highlights, the weekly summary, and the meeting rating.

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
An individual. A name for now; a Ripple user once integrated.

**Team**:
A group of Persons, with exactly one **Team Leader**. Owns its own KPIs, check-ins, headlines, issues, and to-dos. Order in the roster is also the meeting order. Teams are flat; nesting is display-only grouping for now (no rollup, aggregation, or inheritance).

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
The "Include in Weekly Pulse Review" flag on an **Issue** — an *additive pin*. Issue surfacing is **state-only**: an **Open** issue auto-surfaces (any priority — priority never gates surfacing, it is only a label), **Solved** never surfaces, and **Deferred** goes quiet. The pin's job is to **override the quiet** — pulling a **Deferred** issue back into the meeting. The Review shows the **union** of (Open issues) and (pinned Deferred issues). **Headlines are not pinnable**: every headline already surfaces in the Review's news round (see **Headline**), so there is nothing to pin.
_Avoid_: "Low-priority" gating; pinning headlines

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
A **Snapshot entity** capturing the meeting record for a Week. Presented as an **auto-assembled, fully editable draft**, not a blank form: main decisions ← Issues Solved this Week; main actions ← To-Dos created in-meeting this Week; main risks ← Risk-type Headlines + High/Critical open Issues; summary text ← the suggested-format sentence pre-filled from the Week's data. The facilitator confirms or overrides any field. (Mockup pre-fills with plain derivation — no AI required.)

**Readiness**:
A per-Team, per-Week signal with three states, based **only** on KPIs:
- **Not configured** — the Team has no in-meeting-scope KPI definitions (a setup gap; never counts as ready).
- **Not ready** — some in-scope KPIs lack a KPI result for the current Week.
- **Ready** — every in-scope KPI has a KPI result this Week with a status selected (Red counts; readiness means *reported*, not *healthy*).
Check-ins, headlines, and issues do not affect readiness — they are often legitimately empty.

## Scope notes

**No Team "Active" flag.** The team-level active flag (§7) is removed — with a freely-editable roster and no permissions, an inactive team is just one you delete or never add. (The KPI-level **Active** flag is unrelated and kept.)

**Settings — Person management deferred.** Adding and removing **Persons** (and their Team membership) lives in **Settings** and is intentionally **out of the mockup**: it is well-understood CRUD the team already knows how to build, and it answers none of the model questions the prototype exists to probe. (Contrast **KPI** create/edit, which *is* in the mockup precisely because its **Guarded mutation** UX is a live design question.)

**No numeric KPI roll-ups (yet).** KPI History groups and displays weekly results by period — it does **not** sum, average, or otherwise compute over KPI result values, which are free text. Numeric/quarterly aggregation is **out of scope for now** (noted for Andrés): the mockup validates the *history lens*, not computed totals. Revisit only if KPI results gain a structured numeric type.

**No permissions / no auth in the mockup.** The product is fully open — no login, no roles, no per-team edit restrictions — until it is integrated into Ripple. The §8 rules ("a team edits only its own data; Leadership edits everything") and any "Admin" concept are deferred to the Ripple integration, not built in the mockup.

## Flagged ambiguities

**"Leadership"** — used in the spec both as Team #1 (Eder's team, which reports like any other) and as a permission tier ("Leadership can edit everything"). In the mockup there are no permissions, so Leadership is simply a **Team**. The permission meaning is deferred to Ripple.
