---
status: accepted
---

# Check-in is a meeting moment, not a tracked entity

The Weekly Pulse **Check-in** (the "Segue" — each team's opening Win/Concern/Priority/Other note) is removed as a persisted entity. It survives only as the **opening moment of the Weekly Pulse Review**: the team briefly shares personal or professional updates, and **nothing is stored**. The `CheckIn` table and all its rows are dropped; the model, repository, its slot on the Dashboard/Review/History, and the **Check-in → Issue** conversion are deleted. Historical check-ins are deleted with the table — past Meeting History minutes permanently lose their check-in section.

## Why

The original spec modelled the Check-in as a **Snapshot entity** ([ADR 0001](0001-snapshot-vs-living-entities.md)) — one per (Team, Week), upsertable, convertible to an Issue. In practice nothing in the model reads it: **Readiness** is KPI-only, the meeting never looks back at a prior week's check-in, and the segue's whole value is the live conversation, not the stored note. A record no feature consumes is pure carrying cost — schema, a repository, a column on three surfaces, and a conversion path — for data that is write-only.

Anything a check-in *would have* escalated already has a home: a concern worth tracking becomes an **Issue** (raised directly), a win worth broadcasting becomes a **Highlight Headline**. So the segue loses no capture ability by becoming an unsaved moment — it only sheds the storage.

Keeping it "just in case" is the deferred-membership trap: model the thing when a feature actually reads it. Today none does.

## Considered options

- **Keep the entity, hide the UI.** Rejected: still carries the table, repo, migrations, and conversion, and still lets write-only data accumulate — all cost, no reader.
- **Keep historical check-ins read-only in Meeting History, stop new ones.** Rejected: leaves the `CheckIn` model and History rendering alive forever to serve a section no one revisits, and splits the codebase into "old check-ins" vs "no check-ins." The stakeholder confirmed a clean cut — history is allowed to lose the section.
- **Promote to a richer meeting-attendance/segue record.** Rejected as speculative: there is no feature asking for it. Revisit only if the meeting ever needs to *read back* who shared what.
