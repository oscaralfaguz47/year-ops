# OCE Weekly Pulse

A weekly operating dashboard for Oceans Code Experts. Teams add updates *during the week* so that when the Weekly Pulse meeting starts, everything is already organized and ready to review. One set of data, viewed in two modes (Dashboard for input, Weekly Pulse Review for the meeting).

## Language

**Week**:
The Monday–Sunday operating period that organizes all activity. The current Week is created/loaded automatically. Acts as a hard partition for snapshot data and as an origin stamp for living data.
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

**Person**:
An individual. A name for now; a Ripple user once integrated.

**Team**:
A group of Persons, with exactly one **Team Leader**. Owns its own KPIs, check-ins, headlines, issues, and to-dos. Order in the roster is also the meeting order. Teams are flat; nesting is display-only grouping for now (no rollup, aggregation, or inheritance).

**Team Leader**:
The one Person who leads a Team.

**Owner**:
The Person accountable for a specific record — e.g., the owner of a KPI, an Issue, or a To-Do. Defaults to the Team Leader but can differ. This is a single concept; do not split it into "owner/leader" on records.

**KPI definition**:
A recurring KPI set up once per Team (name, owner, weekly target, in-meeting-scope flag, active flag). Configured in Settings. Minimum 2 per Team.

**KPI result**:
A **Snapshot entity** — one Team's actual result for one KPI in one Week, with a manually chosen status (Green / Yellow / Red) and notes. Exactly one result per (KPI, Week): if it exists it is updated, never duplicated.

**In meeting scope** (KPI flag):
The "Include in Weekly Pulse Review" flag on a **KPI definition** — a *scope gate*. A KPI out of scope never appears in the Review regardless of status. Among in-scope KPIs, the surfacing rules decide emphasis (Red/Yellow/missing dominate; Green stays quiet).

**Pin to Review** (Issue / Headline flag):
The "Include in Weekly Pulse Review" flag on an Issue or Headline — an *additive pin*. The Review surfaces the **union** of (items auto-surfaced by status rules) and (items manually pinned). Lets a Low-priority issue or quiet headline be pulled into the meeting deliberately.

**Conversion**:
Turning one record into a new living entity, always **additive**: a check-in or headline → a new Issue; an Issue → a new To-Do. The new record is pre-filled from the source and holds a back-reference to its origin (origin type + id). The **source is always preserved** — conversions never consume or delete it. Snapshot sources (check-ins, headlines) stay intact in their Week.
_Avoid_: "convert" implying the source disappears

## Scope notes

**No permissions / no auth in the mockup.** The product is fully open — no login, no roles, no per-team edit restrictions — until it is integrated into Ripple. The §8 rules ("a team edits only its own data; Leadership edits everything") and any "Admin" concept are deferred to the Ripple integration, not built in the mockup.

## Flagged ambiguities

**"Leadership"** — used in the spec both as Team #1 (Eder's team, which reports like any other) and as a permission tier ("Leadership can edit everything"). In the mockup there are no permissions, so Leadership is simply a **Team**. The permission meaning is deferred to Ripple.
