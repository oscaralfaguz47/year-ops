# OCE Weekly Pulse

A weekly operating dashboard for Oceans Code Experts. Teams add updates *during the week* so that when the Weekly Pulse meeting starts, everything is already organized and ready to review. One set of data, viewed in two modes (Dashboard for input, Weekly Pulse Review for the meeting).

## Language

**Week**:
The Monday–Sunday operating period that organizes all activity, with boundaries in **Costa Rica time (UTC-6, year-round — no DST)**. The current Week is created/loaded lazily on first access after the Monday 00:00 CR rollover. Acts as a hard partition for snapshot data and as an origin stamp for living data. In Ripple a Week is a **`WeekStart` value** (the Monday date, computed in CR time), **not its own table**: every **Snapshot entity** carries a `WeekStart` column and every **Living entity** stamps an `OriginWeekStart`. The "current Week" is `WeekStart(now)` — there is no row to create and no manual advance. Ripple has no prior week concept — its native cadence is biweekly pay periods — so the CR-time Monday boundary is new and must be computed consistently everywhere by one shared function (KPI History's **Period** grouping reuses the same authoritative Monday). Promote `Week` to a persisted entity only when it first earns an attribute of its own.
_Avoid_: "Meeting record", "L10", "session"; deriving week boundaries ad hoc at each call site; biweekly/pay-period cadence

**Snapshot entity**:
A record that belongs to exactly one Week and is genuinely blank at the start of each new Week. The stored snapshots are KPI results and headlines/highlights. (The **Weekly Summary** is a *derived* read-only view over a Week's data, not a stored snapshot in v1; the meeting rating is deferred. **Check-in** was formerly a snapshot entity but is no longer stored — see [ADR 0003](../adr/0003-check-in-is-a-meeting-moment-not-an-entity.md).)

**Living entity**:
A record created *in* a Week (stamped with its origin Week) that remains active and resurfaces in every subsequent Weekly Pulse Review until it reaches a terminal state. It is never re-created or copied forward — it persists. Issues and To-Dos are the living entities.
_Avoid_: copying/cloning a living entity into a new week

**Issue**:
A **Living entity** — a problem needing discussion, alignment, a decision, or action, worked through the IDS framework (Identify, Discuss, Solve). Terminal state: Solved. Surfaces on both the Dashboard (running open list) and the Review (filtered) until solved.

**To-Do**:
A **Living entity** — a single action with exactly one owner and a due date. Terminal state: Done. Surfaces on both the Dashboard and the Review until done.
_Avoid_: "task" as a separate concept, "action item"

**Dashboard**:
The **authoring surface** — the working board where teams populate the Week *during the week*, so the meeting opens with everything already organized. Shows the full **unfiltered** working set (a running open list, not surfaced/loud-first) as a **Kanban board of editable cards**: clicking a card opens it for editing in place. Edit scope by type — a **KPI result**'s weekly value/status only (the KPI *definition* is edited in Settings, never here); Headlines, Issues, and To-Dos fully editable including their **Team**.
_Avoid_: a read-only Dashboard (cards must be editable); editing KPI *definitions* here; applying Review surfacing/filtering to the Dashboard

**Weekly Pulse Review**:
The **meeting surface** — a **guided, sequenced walk** through the Week in Team order, run as a script moment by moment (**not** a board, and **not** a filtered clone of the Dashboard). Shows only **surfaced** data (loud-first). Each moment carries its own **edit rule**: (1) **Check-in** — an unsaved personal/professional sharing moment, nothing stored; (2) **KPI Review** — **view-only**; (3) **Headlines** — **view-only**, the single allowed action being *drop a headline to an Issue* (the L10 "drop it down to the issues list" move — headlines never spawn a To-Do directly; see **Conversion**); (4) **Issues** — **fully editable live**; (5) **To-Dos** — **fully editable live**, created during the meeting from Issues. KPIs and Headlines are captured *during the week* and are read-only in the meeting; Issues and To-Dos are the meeting's live working moments.
_Avoid_: a "[Kanban | List]" view switch or a Dashboard "meeting mode" toggle (both dropped — the Review is only the guided walk); editing KPIs or Headlines inside the Review; treating the Review as a second editable board

**Deferred** (Issue state):
Consciously parked. The Issue stays on the Dashboard's open list but is excluded from default Review surfacing until reactivated (optionally auto-reactivated on a chosen revisit Week). Deferred goes *quiet*.

**Blocked** (To-Do state):
Cannot proceed. The To-Do keeps surfacing in the Review — being blocked is what the meeting needs to clear. Blocked stays *loud*. Non-terminal.

**Check-in** (Segue):
The **opening moment of the Weekly Pulse Review** — *not a stored entity*. The team briefly shares personal/professional updates and **nothing is saved**: no table, no card, no conversion. Anything worth keeping escalates through an existing path — a concern becomes an **Issue** (raised directly), a win becomes a **Highlight Headline**. See [ADR 0003](../adr/0003-check-in-is-a-meeting-moment-not-an-entity.md). (Formerly modelled as a Snapshot entity — one per (Team, Week), Win/Concern/Priority/Other + note, convertible to an Issue; that entity is removed.)
_Avoid_: persisting a check-in; a check-in → Issue conversion; making Readiness depend on check-ins

**Headline**:
A **Snapshot entity** — a short piece of weekly news for a Team, of one type: **Highlight** (a win to celebrate) or **Risk** (a concern to flag). Belongs to exactly one Week and is blank at the start of the next. In the **Weekly Pulse Review** the team's headlines open its segment as a quick **news round**: *all* of the week's headlines surface (it's the meeting's good-news/bad-news round, EOS-style), with **Risk** flagged *loud* and **Highlight** kept *quiet*. Can be converted into an Issue (additive — the headline is preserved). Feeds the Weekly Summary's risks (Risk-type only).
_Avoid_: treating a headline as living — it does not carry forward

**Person**:
An individual who can own or lead Weekly Pulse records. In Ripple, a Person **is an `ApplicationUser`** (the login identity — name, email, photo, auth), referenced by its user id. **Owner**, **Team Leader**, and a To-Do's owner are all `ApplicationUser` references. Pulse deliberately does **not** route through `ConsultantDetail` (the HR/employment record): Pulse needs identity, not payroll/PTO data, and anchoring on `ConsultantDetail` would wrongly exclude any login user (e.g. a Leadership facilitator) who is not a billable consultant.
_Avoid_: keying a Person on `ConsultantId`

**Team**:
A functional org unit (e.g. Sales, Marketing, Operations, Leadership) grouping Persons, with exactly one **Team Leader**. Owns its own KPIs, headlines, issues, and to-dos. Order in the roster is also the meeting order. Teams are flat; nesting is display-only grouping for now (no rollup, aggregation, or inheritance). In Ripple a Team is a **new first-class entity** — Ripple has no pre-existing org-unit concept (only delivery-oriented `Project`/`Client`, which Pulse does **not** derive Teams from). It is **owned by the WeeklyPulse area** but modeled as a generic `Team` (not `PulseTeam`) so it can later graduate into a shared, company-wide org concept. See [ADR 0002](../adr/0002-team-as-new-org-entity.md).

In v1 a Team is **leader-only**: `{ Name, Team Leader (`ApplicationUser`), display order }` — there is **no Person↔Team membership roster**, because nothing in the model reads one (Owners may be cross-team and are picked from the full `ApplicationUser` directory; meeting order is *Team* order; Readiness is KPI-only). "Grouping Persons" is conceptual, not a stored member list. Add membership later only when a feature actually consumes it (e.g. a "who's on this Team" view).
_Avoid_: deriving a Team from `Project`/`ProjectConsultantAssigned`; naming it `PulseTeam`; modeling a member roster nothing reads

**Team Leader**:
The one Person who leads a Team. A Team has exactly one Leader, but a Person may lead **more than one Team** (e.g. David Barrios leads both Sales and Marketing).

**Owner**:
The Person accountable for a specific record — e.g., the owner of a KPI, an Issue, or a To-Do. Defaults to the Team Leader but can differ, and may be a Person from a **different Team** than the record's `Team`. Grouping everywhere (Dashboard, Review, History) follows the record's **Team**, never the Owner's team. This is a single concept; do not split it into "owner/leader" on records.

**KPI definition**:
A recurring KPI set up once per Team (name, owner, weekly target as **free text** — e.g. "≥ 95%", "< 2 days", "$50k"). Configured in Settings. Minimum 2 per Team. Carries **one** flag — **active** (live vs retired). (Review inclusion is no longer a definition flag: it moved to a per-week decision on the **KPI result** — see **In meeting scope**.) A KPI definition is **structural and stable** — it is *not* an everyday edit like a KPI result or a living entity.

**Guarded mutation** (KPI definition):
Creating, editing, or retiring a **KPI definition** is **guarded** — it requires an explicit confirmation ("you're about to create/change a KPI…"). The guard is deliberate friction that signals KPI definitions are **structural**, changed rarely, *unlike* the frictionless everyday edits: a **KPI result** (the weekly value), **Issues**, and **To-Dos** all change freely with no confirm. The guard reinforces the mental model, not a permission check (there are no permissions in the mockup).

**Active** (KPI flag):
Whether a KPI definition is **live** or **retired**. A retired (inactive) KPI stops expecting weekly results — it no longer prompts for input and doesn't count toward **Readiness** — but its historical KPI results stay intact. **Active is now the sole KPI-definition flag** and the basis of **Readiness** (every Active KPI is expected to report each Week). Whether an Active KPI's result actually surfaces in a given meeting is a *separate, per-week* decision — see **In meeting scope**.

**KPI result**:
A **Snapshot entity** — one Team's actual result (**free text**) for one KPI in one Week, with a manually chosen status (Green / Yellow / Red), notes, and an **Include in Weekly Pulse Review** flag (the per-week scope gate — see **In meeting scope**; defaults to included). No arithmetic comparison to target — status is judgment. Exactly one result per (KPI, Week): if it exists it is updated, never duplicated.

**In meeting scope** (per-week KPI-result flag):
The **"Include in Weekly Pulse Review" checkbox on a KPI result** — a *per-(KPI, Week) scope gate*, chosen when the weekly value is recorded/edited on the Dashboard, **not** a structural property of the definition. The gate is per-week: the same KPI can be reviewed one Week and skipped the next. **Defaults to included** (curate-*out*: recording a value includes it; untick to keep it off this week's agenda) so the Review is never silently empty. The Review's KPI moment shows exactly this Week's *included* results; among those, surfacing decides emphasis (Red/Yellow/missing loud, Green quiet — dim, don't hide). An un-included result — or a KPI with no result this Week — simply doesn't appear. Independent of **Readiness**, which keys off **Active** and ignores inclusion.
_Avoid_: treating review-inclusion as a definition-level flag; letting inclusion affect Readiness; a default-unchecked checkbox (silently drops KPIs from the meeting)

**Pin to Review** (Issue flag):
The "Include in Weekly Pulse Review" flag on an **Issue** — an *additive pin*. Issue surfacing is **state-only**: an **Open** issue auto-surfaces (any priority — priority never gates surfacing, it is only a label), **Solved** never surfaces, and **Deferred** goes quiet. The pin's job is to **override the quiet** — pulling a **Deferred** issue back into the meeting. The Review shows the **union** of (Open issues) and (pinned Deferred issues). Because pinning only does real work on a Deferred issue (Open ones surface anyway, Solved ones are done), the pin affordance is **offered only on Deferred issues** — it reads as *"un-park this for the meeting."* **Headlines are not pinnable**: every headline already surfaces in the Review's news round (see **Headline**), so there is nothing to pin.
_Avoid_: "Low-priority" gating; pinning headlines; offering pin on Open/Solved issues

**Conversion**:
Turning one record into a new living entity, always **additive** and **one rung at a time**: a **headline → an Issue**, then an **Issue → a To-Do**. There is **no `headline → To-Do` shortcut** — news becomes an action only by first becoming a discussable Issue, so **every converted To-Do descends from an Issue** (it was discussed before it was owned). The full escalation ladder is *headline → Issue → To-Do*. The new record is pre-filled from its source and holds a back-reference to its origin (origin type + id), so a To-Do reads back as `To-Do → Issue → Headline` — a legible provenance chain (*news → discussion → action*). The **source is always preserved** — conversions never consume or delete it; a headline stays intact in its Week.
_Avoid_: a `headline → To-Do` shortcut; a `check-in → Issue` conversion (check-in is no longer an entity); "convert" implying the source disappears

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
- **Not configured** — the Team has no **Active** KPI definitions (a setup gap; never counts as ready).
- **Not ready** — some **Active** KPIs lack a KPI result for the current Week.
- **Ready** — every **Active** KPI has a KPI result this Week with a status selected (Red counts; readiness means *reported*, not *healthy*).

Readiness keys off **Active** only — it is deliberately **decoupled from per-week Review inclusion** (a KPI can be reported-and-ready yet ticked off this meeting's agenda, or included yet unreported).
Headlines and issues do not affect readiness — they are often legitimately empty.

## Scope notes

**No Team "Active" flag.** The team-level active flag (§7) is removed — with a freely-editable roster and no permissions, an inactive team is just one you delete or never add. (The KPI-level **Active** flag is unrelated and kept.)

**Settings scope.** Pulse **Settings** manages **Teams** (name, Team Leader, order) and **KPI definitions** (the **Guarded mutation** UX — a live design question, hence in the mockup). It does **not** manage **Persons**: in Ripple, Persons are existing `ApplicationUser`s administered by the AdminCenter user admin, so Pulse just references them. Person↔Team membership is not modeled (see **Team**). (In the mockup, Person management was out of scope as well-understood CRUD that probes none of the model questions.)

**No numeric KPI roll-ups (yet).** KPI History groups and displays weekly results by period — it does **not** sum, average, or otherwise compute over KPI result values, which are free text. Numeric/quarterly aggregation is **out of scope for now** (noted for Andrés): the mockup validates the *history lens*, not computed totals. Revisit only if KPI results gain a structured numeric type. **Planned later (§5 change request):** make KPIs carry a **KPI type**, configured in Settings, so that *aggregable* KPIs (those whose values are structured/numeric) can roll up into an aggregated month/quarter view — only KPIs whose type makes sense to aggregate would appear there. This is the "structured numeric type" trigger above; not built yet.

**Permissions (Ripple integration).** The mockup is fully open. Inside Ripple, access is gated through Ripple's **claims/policy** pattern (a new `WeeklyPulse` `SystemSubArea`), at **coarse granularity**: two policies — **Participate** (view + everyday edits: KPI results, headlines, issues, to-dos for any team) and **Administer** (the guarded/structural actions: KPI definition create/edit/retire, deleting Meeting History, Settings/Person management, and granting Participate — see below). Editing is **trust-based, not row-level team-scoped** — a Participant may edit any Team's data. Row-level "a team edits only its own data" enforcement is intentionally **not** built: it would require a load-bearing Person↔Team membership (deferred with Settings/Person management) and conflicts with the cross-team **Owner**. Revisit only if abuse appears.

**Granting Participate to non-admins (§6 change request).** Weekly Pulse users are ordinary employees who must *not* be administrators. Participate is therefore granted through a **dedicated, seeded, non-admin role — `Weekly Pulse Participant`** — that carries **only** the `WeeklyPulseParticipate` claim. A **Master or Admin** grants access by adding a user to that role through Ripple's **existing role/user-assignment UI** (no new Pulse-native screen). This **decouples participation from administration**: previously the only seeded carriers of the Participate claim were the **Master** and **Admin** roles, so Pulse access implied being an admin — the new role is what breaks that. **Administer** stays with Master/Admin. *Chosen over per-user direct claim grants deliberately:* the app is **100 % role-based** (`ApplicationUserClaim` is used only for profile image + 2FA, never access), and no per-user "grant feature access" UI exists — a role reuses the existing UI and keeps a single, role-auditable access model, whereas per-user claims would need new UI and a parallel access model nothing else uses.
_Avoid_: granting Pulse access by making someone **Admin**; a second per-user access model via `ApplicationUserClaim`; a bespoke Pulse-only access-granting screen when the role/user-assignment UI already exists

## Flagged ambiguities

**"Leadership"** — used in the spec both as Team #1 (Eder's team, which reports like any other) and as a permission tier ("Leadership can edit everything"). On Ripple integration these **cleanly separate**: **Leadership-the-Team** is an ordinary `Team` row that reports like any other; **Leadership-the-authority** is the **Administer** policy/claim and is *not* tied to membership in the Leadership Team. (Resolved — no longer ambiguous.)
