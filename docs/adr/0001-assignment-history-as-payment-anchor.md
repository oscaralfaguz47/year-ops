# Assignment history is the payment anchor for non-hours payments

**Status:** accepted

## Context

Feature 1 lets a consultant appear on the Payment Sheet for *any* payable item in a period
(interviews, debits/credits, reimbursements), not only when they have an active project assignment.
But pricing and accounting for those lines both need a project context: interviews carry only a
duration and must be priced at an hourly rate, and every payment line needs a cost-center /
accounting account, which is resolved from the consultant's **position on a project**
(`SP_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_GetCurrentHistory` → `CONSULTANT_POSITIONS_ACCOUNTING_CONFIGURATION`).

## Decision

We do **not** build a dedicated code path for consultants with no project context. Instead, each
non-hours payment line anchors to the consultant's **most-recent historical assignment**
(`PROJECTS_CONSULTANTS_ASSIGNED_HISTORY`, `ActionDate <= EndDate`, regardless of `IsActive`), which
supplies the rate, position, and accounting config from already-stored data. The displayed Payment
Sheet row is project-less ("—", hours = 0), but under the hood the lines bind to that historical
project.

For a consultant who has **never** had an assignment (e.g. someone joining purely for interviews),
there is no anchor and no auto-compute is possible. The operational procedure is to **create a
project, assign the consultant to it** (leaving them assigned, or unassigning afterward — the anchor
SP reads inactive history too), which seeds the rate + position + accounting config. No special
in-app handling is built for the zero-anchor state beyond an empty-state message that points the
operator to this procedure.

## Considered options

- **Build a no-history code path** — a manual rate prompt plus accounting-config selection UI for
  project-less consultants. Rejected: significant UI/data work for a rare case, and it duplicates
  configuration that the project/position model already expresses correctly.
- **Synthetic placeholder project baked into the schema** — rejected for the row-*grouping* concern
  (we keep rows genuinely project-less), but an operator-created internal project is the sanctioned
  manual remedy for the zero-anchor case.

## Consequences

- Reversible only by building the no-history path we skipped — meaningful cost.
- The assign/unassign trick is sensitive to dates: the assignment history must predate the period
  being paid (`ActionDate <= EndDate`), or the anchor resolves to nothing.
- Assigning to a *real client's* project pollutes that client's roster; prefer a dedicated internal
  project for genuinely-non-project people.
- The rate fallback must pick the most-recent record **with salary > 0**, not merely the most-recent
  record (an unassignment row may zero the salary).
