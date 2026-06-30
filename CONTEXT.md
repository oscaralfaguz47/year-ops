# Ripple by Oceans — Context Glossary

A glossary of the domain language used in Ripple. Implementation details live in code and ADRs, not here.

## Payment Sheet

The screen listing consultants to pay for a pay period. **Not a persisted record** — the
underlying `PaymentSheet` table was dropped (migration `20240517163129`). It is a virtual,
on-demand view recomputed per request by `SP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters`.

## Consultant Row

A single consultant's entry on the Payment Sheet for a period. Historically a row existed only
when the consultant had an **active, paid project assignment** in the period (the `ActiveConsultants`
set). **Decided change (Feature 1):** a row also appears whenever the consultant has *any payable
item* in the period, even with zero active assignments. The merged view stays a single sheet — there
is no separate "non-hours payments" screen.

A consultant with payable items but **no active assignment** that period gets a single
**project-less consultant-level row** (project shown as "—", hours = 0) that aggregates all their
non-hours payable items. Consultants who do have an assignment are unchanged — their non-hours items
keep appending to the existing project row. The new row-source therefore fires *only* when active
assignments = 0, so there is no double-counting.

The row fires for **any non-rejected** payable item (the same gate the pricing SPs use), and shows
**Pending** ("to be paid", three-dots, editable) until paid through the normal pay flow, after which
it shows **Paid** (the `!` icon). *Manually* marking a record Paid without running a payment is a
separate concern (Feature 2), out of scope here.

## Payable Item

Anything owed to a consultant in a period. Categories:
- **Project hours** — reported time movements (`REPORTING_MY_TIME_MOVEMENTS`), grouped into an
  approved submission, priced at the consultant's rate.
- **Interview** — technical-interview work owed to a consultant. Stores only duration, no amount,
  so it must be priced at an hourly rate. The rate (and the accounting position) come from the
  consultant's **most recent historical assignment** (see [Payment Anchor](#payment-anchor)). The
  line is editable as an override.
- **Debit / Credit** — manual accounting adjustments (`CONSULTANT_PAYMENTS_DEBITS_CREDITS`),
  where "Credit" is a specific `TransactionType`. Do **not** confuse with Priscila's informal use of
  "credits" to mean *everything owed* — that broader idea is **Payable Items** (see below).
- **Reimbursement** — benefit reimbursements (e.g. spread over months) and expense reports.

Interviews and reimbursements are *already* priced as payment lines for consultants who have a row;
Feature 1 lets the payable item itself produce the row.

The breakdown of a consultant's payable items (each labeled by type and amount) is shown in the
existing **payment-detail drill-down**, not as a new column on the sheet row. Canonical name for the
union of these lines is **"Payable items"** — preferred over Priscila's informal "credits", which
collides with the accounting Credit type.

## Payment Anchor

The project a non-hours payment line binds to for pricing and accounting. Because interviews,
debits/credits, and reimbursements have no `ProjectId`, each line anchors to the consultant's
**most recent historical assignment** (`PROJECTS_CONSULTANTS_ASSIGNED_HISTORY`, most recent record
at/before period end, active or not). That anchor supplies the hourly rate, the position, and the
cost-center / accounting account. The Payment Sheet row stays project-less for display; the anchor
is an under-the-hood concern. A consultant with no assignment history has no anchor — see
[ADR 0001](docs/adr/0001-assignment-history-as-payment-anchor.md) for how that zero-anchor case is
handled (operator creates/assigns a project; no special in-app code path).

## Manual Hours Upload

An admin submitting **project hours on behalf of a consultant** who cannot self-report (extended/
maternity leave, vacation) for a given pay period (Feature 3). It is a thin admin-facing front door
onto the *existing* time-reporting flow: it produces ordinary reported time (`REPORTING_MY_TIME_MOVEMENTS`)
grouped into a normal `ReportingMyTimeMovementSubmission`, and from there follows the standard path
(review for payment → approve → pay).

Critically it is the **mirror image** of a project-less [Consultant Row](#consultant-row): manual
hours upload **requires an active assignment** to the chosen project (the normal submission validation
rejects an unassigned consultant), so it has **nothing to do with the [Payment Anchor](#payment-anchor)**.
A consultant with no active assignment is out of scope (assign them first, per ADR 0001). Scoped **per
[pay period](#pay-period-quincena)**, not one lump for a whole absence.

It is modelled as **autofill performed by an admin on someone else's behalf**: it reuses the existing
autofill behaviour plus the submit step, so it lands at **"Waiting to be approved"** and goes through
normal review — it is **never auto-approved**. The admin enters a **period total**, which defaults to
the period's [workable days](#workable-days) × 8 h and is overridable; the total is spread evenly across
those workable days to the cent, with the last few days carrying any rounding remainder so the movements
sum to exactly the entered total. The admin never enters holiday hours — paid holidays are paid automatically by the
payment computation, separately from this upload (see [Workable Days](#workable-days)).
Because autofill only operates on **no-tracking-tool projects** (it rejects tracking-tool projects),
manual hours upload is scoped to no-tracking-tool assignments. **Tracking-tool projects are out of
scope** — their submit requires consultant-supplied evidence screenshots an admin cannot produce —
and remain an operator/manual case (same spirit as ADR 0001). No evidence-file gate is involved.

## Workable Days

The set of **weekdays** (Mon–Fri) in a pay period that an on-behalf [Manual Hours Upload](#manual-hours-upload)
fills normal hours on — concretely, the days `WeekdaySpread.GetWeekdayDates` produces for that
consultant/project/period. It is both the basis for the upload's default total (workable days × 8 h) and
the divisor the entered total is spread across; computed **server-side** so the displayed count, the
default, and the actual spread can never diverge.

Holidays are handled by the *paid-holiday* rule, not by a separate count:

- On a **holiday-paying assignment** (`IsDefaultProject && HolidaysMustBePaid`), the consultant's
  holidays are **excluded** from workable days — they are *not* filled with normal hours here, because
  the payment computation already injects a separate paid **"Holidays"** movement
  (`NumHoursForHoliday`, default 8 h) for each. Filling them here too would pay the day twice.
- On a **non-paying assignment**, holidays are ordinary workdays and are **included** in workable days
  (filled as normal hours).

This is the inverse of "paying ⇒ add the holiday hours," and it is deliberate: paying assignments get
the holiday paid through the automatic channel, non-paying ones get it as worked time — each paid once.

## Pay Period (quincena)

The biweekly cadence the sheet is computed for (`CONSULTANT_DETAILS.PaymentPeriod = 1`).

## Time Off

The umbrella for leave requests, each a `TimeOffRequest` with a `TimeOffType`. Three types exist:

- **PTO (Paid Time Off)** — paid vacation. Has a **balance** that is consumed by requests. Two
  populations compute it differently:
  - *Consultant PTO* — a flat annual allowance (`AnnualPaidTimeOffDays`) minus used.
  - *Administrative PTO* — a **carried-over** opening balance (`InitialAdminPtoBalance`, the figure
    brought over from the external **Vacation Tracker** tool) **plus** an amount **accrued
    month-to-month** in Ripple (one day per month from an anchor date), minus used. Only the
    Administrative population sees the carried-over-vs-accrued breakdown.
- **UPTO (Unpaid Time Off)** — unlimited; no balance.
- **VTO (Voluntary Time Off)** — a single volunteering day per year (a fixed `1 − used` allowance;
  configurability was considered and withdrawn, see ADR 0003). **Not** a vacation balance and
  unrelated to PTO carry-over/accrual. Distinct concept that merely shares the word "VTO" with an
  unrelated static perks blurb — when this glossary says VTO it means the Voluntary Time Off request
  type.

**Used** is never stored; it is the sum of a consultant's request days in scope, and counts both
**Approved and pending ("Waiting to be approved")** requests against the balance.

## Time Off Balances card

The single card on the Time Off page that shows a user their balances. It renders one of two shapes
depending on the viewer: the **Administrative-PTO** shape (carried-over + accrued breakdown, used,
monthly rate) or the **consultant** shape (PTO line, UPTO unlimited, and the VTO line). Priscila's
informal **"Policies and PTO" card** ("polisis/pitio") refers to this card. The planned changes:
give **VTO its own card** on the Time Off page (same audience as today — no auth/visibility change;
Feature 5), and **collapse the PTO carried-over/accrued split into a single current-balance figure**,
keeping **used** and **monthly rate** (Feature 6, Administrative population only).
