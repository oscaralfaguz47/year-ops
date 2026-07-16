---
status: accepted
---

# Team as a new, Pulse-owned (but generic) org entity

Weekly Pulse groups all of its data — KPIs, check-ins, headlines, issues, to-dos — under a **Team** (Sales, Marketing, Operations, Leadership). Ripple has no org-unit concept today. We model **Team as a new first-class entity**, **owned by the WeeklyPulse area** but named and shaped generically (`Team`, not `PulseTeam`) so it can later be promoted into a company-wide organizational concept without a rebuild.

## Why

Ripple's only existing groupings are `Project` and `Client` — both delivery-oriented. A Pulse Team is a **functional department**, not a client engagement: a person sits on many Projects but in one Pulse Team; Projects come and go per client, while departments are stable; and the Team roster carries **meeting order**, which Projects do not. Deriving Teams from `Project`/`ProjectConsultantAssigned` would therefore be a category error and would fight the domain model.

Keeping the entity generic (rather than `PulseTeam`) costs nothing now and preserves the option for Ripple to adopt one canonical Team later — Pulse just happens to be the first feature that needs org structure.

## Considered options

- **Derive Team from `Project` + `ProjectConsultantAssigned` (lead = `SuccessManagerId`).** Rejected: conflates client-delivery structure with functional departments; no stable roster or meeting order; breaks when a person is on many projects.
- **Model a `PulseTeam` entity scoped tightly to the feature.** Rejected: bakes "weekly pulse" into what is really the org's first Team concept, forcing a rebuild/migration if Team graduates app-wide. We get the same isolation by *owning* a generically-named `Team` in the WeeklyPulse area.
- **Build a shared org/Team module up front, used by multiple areas.** Rejected for now: speculative — no other area needs it yet, and designing for unknown consumers would over-engineer the first use. The generic naming keeps promotion cheap if the need materializes.
