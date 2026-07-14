# OCE Weekly Pulse — Change Requests

**Context:** The original spec is already implemented. This document lists **only the new changes** to make on top of the current build (https://oce-pulse-gather.lovable.app/).

---

## 1. Dashboard — cards must be editable
Clicking any card must open it for editing, not just viewing (today they are view-only). What's editable depends on the type:

- **KPI:** only the weekly value/status for that period. The KPI definition itself is **not** editable here.
- **Headline / Highlight:** fully editable — type, headline text, and team.
- **Issue:** fully editable — priority, status, title, and team.
- **To-Do:** fully editable — owner, title, team, and due date.

## 2. Remove Check-in as an entity
Check-in is **not** an entity that needs tracking — it's just a moment of the review. Remove it completely from the application (Dashboard, Review, History, data). It survives only as the opening moment of the Review meeting (see §3), where the team briefly shares personal or professional updates; nothing is saved.

## 3. Weekly Pulse Review — guided meeting with per-moment edit rules
The Review must work as a **guide for the meeting**, moment by moment:

1. **Check-in** — brief personal/professional sharing, short time. Nothing to edit or store.
2. **KPI Review** — **view only.** Neither the weekly update nor the KPI itself can be modified here.
3. **Headlines** — **view only.** The single allowed action is **dropping a headline to an Issue** (the escalation ladder is `headline → Issue → To-Do`; headlines never spawn a To-Do directly). _Amended during grill — as originally written this said "creating a To-Do from a headline"; the strict ladder replaces the shortcut. See CONTEXT.md **Conversion**._
4. **Issues** — **fully editable live.** Issues are their own moment of the meeting; as discussion happens they get edited, and To-Dos get created from them.
5. **To-Dos** — **fully editable live.** New To-Dos get created during the review (from issues, or raised standalone) and edited as needed. _(Headlines reach a To-Do only via an Issue — see §3.3 amendment.)_

Rationale: Issues and To-Dos are the live working moments of the review, so they must be editable there; everything captured during the week (KPIs, Headlines) is read-only in the review.

## 4. KPI — "Include in Weekly Pulse Review" checkbox
When updating a KPI on the Dashboard, add a checkbox labeled **"Include in Weekly Pulse Review"** (include in the weekly). When the Review starts, the KPI Review page shows **only the KPIs whose checkbox is marked** for that week.

## 5. KPI types & aggregated views (later)
KPIs should become configurable from **Settings** to define their type, so that aggregable KPIs can be rolled up into an aggregated view — e.g., monthly, quarterly. Only the KPIs that make sense to aggregate would appear there. (Planned as a later addition, not immediate.)

## 6. Access without an administrative role
The people who will use this dashboard do **not** have an administrative role. The app needs its own way to grant them access: we need to identify these users and give them access to the resource directly, independent of admin roles. Define the identification/authorization mechanism as part of this work.
