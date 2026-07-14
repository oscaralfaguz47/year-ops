# Manual hours upload is admin-driven autofill, scoped to no-tracking-tool projects

**Status:** accepted

## Context

Feature 3 lets an admin submit project hours **on behalf of** a consultant who cannot self-report
for a pay period (extended/maternity leave, vacation; Priscila's upcoming ~4-month leave is the
driving case). Today the only way hours reach the system is the consultant's own time-reporting flow
in the TrackingTool area, which branches on `Project.ClientHasTrackingTool`:

- **No-tracking-tool projects** — manual hours; `AutofillTimeEntryTrackingTool` spreads an entered
  daily quantity across the period's weekdays. Submit (`CreateSubmission`) only requires `hours > 0`.
- **Tracking-tool projects** — time from/to; submit **requires uploaded evidence screenshots** from
  the consultant's tracking tool(s) (`CreateSubmission`, the `if (project.ClientHasTrackingTool)`
  branch).

An admin standing in for an absent consultant has no access to that consultant's tracking-tool
screenshots, so the evidence gate cannot be satisfied on their behalf.

## Decision

Model manual hours upload as **autofill performed by an admin on someone else's behalf**: reuse the
existing autofill spread + the submit step, landing the result at **"Waiting to be approved"** so it
flows through normal review → approve → pay. It is **never auto-approved** (we do *not* reuse the
`RemoveProjectConsultantInPeriod` "Approved" direct-insert shortcut — a human still reviews
admin-keyed hours).

Because autofill itself only operates on **no-tracking-tool projects** (it rejects tracking-tool
projects outright: `if (project == null || project.ClientHasTrackingTool) return failure`), manual
hours upload is **scoped to no-tracking-tool assignments**. **Tracking-tool projects are out of scope**
— a consultant on one who is absent remains an operational/manual case (same spirit as ADR 0001's
zero-anchor handling). No evidence-file gate is bypassed, because the feature never reaches it.

The endpoint lives in `Finances/PaymentSheetsController` behind the existing
`AccessToManageTheBasicsOfPaymentSheets` policy (alongside the other admin write-time precedents); no
new policy/claim is introduced. It requires an **active assignment** to the chosen project (reusing
the normal submission validation), so it is the mirror image of — and unrelated to — the project-less
[Payment Anchor](../../CONTEXT.md) path. Movements/submission key to the **subject** consultant
(including their `PaymentPeriod`); the **acting admin** is recorded via `UserIdLastUpdatedBy` plus a
`ReportingMyTimeComments` "uploaded on behalf of" marker.

## Considered options

- **Build an evidence-handling path for tracking-tool projects** — e.g. let the admin attach files
  or check a "no evidence available" override that bypasses the gate. Rejected for now: it weakens an
  audit control for a case the driving users don't have (the absent consultant's screenshots don't
  exist to attach), and adds UI/validation-bending for a minority of leave situations.
- **Reuse the `RemoveProjectConsultantInPeriod` "Approved" direct-insert** — lands straight at
  Approved, skipping review. Rejected: admin-entered hours nobody else witnessed are exactly where
  segregation of duties matters most; keep a human approval step.
- **A dedicated TrackingTool-area endpoint under the self-report policy** — rejected: that policy
  (`BasicAccessToReportingMyTime`) is the consultant's own gate, wrong for an admin acting on others;
  and the PRD places the control in the payment-sheet "actions" family.

## Consequences

- A consultant assigned to a **tracking-tool** project who is on leave cannot have hours uploaded via
  this feature — handled manually until/unless the evidence path is built. Reversing this scope means
  building exactly the evidence path we deferred — meaningful cost.
- The collision guard is inherited unchanged: an already-submitted (non-rejected) period is blocked
  (`ValidateSubmission`); a rejected one is re-submitted; drafts are overwritten. Correcting an
  already-approved period stays the separate `EditHoursFromApprovals` concern.
- Reminders self-suppress (a submission now exists), but if the reminder job runs before the admin
  uploads, the on-leave consultant may still receive one "you owe hours" email — accepted; no
  leave-aware suppression is built.
</content>
</invoke>
