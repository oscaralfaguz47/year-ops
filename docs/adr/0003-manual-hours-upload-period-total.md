# Manual hours upload takes a period total (workable-days × 8 default), spread with a last-day remainder

**Status:** accepted

**Amends:** [ADR 0002](0002-manual-hours-upload-as-admin-autofill.md) — the on-behalf upload's *input
unit*. Everything else in 0002 (admin-on-behalf model, lands at "Waiting to be approved", no-tracking-
tool scope, audit trail) stands unchanged.

## Context

ADR 0002 shipped Manual Hours Upload reusing the consultant autofill's input shape: the admin types a
**daily quantity** (`HoursPerDay`, default 8) and the backend writes it to every weekday in the period
(`WeekdaySpread.GetWeekdayDates`, skipping weekends and — on paying assignments — holidays). The admin
never sees how many days that is or what the period total comes to; they enter a per-day rate and trust
the spread.

Finance admins reason in **period totals** ("this consultant is owed N hours for the quincena"), not
per-day rates. The driving request: default the upload to the period's expected full-time hours and
show that figure, while still allowing an override to add or drop a few hours.

The period total is `workableDays × 8`. That number cannot be computed client-side: the workable-day
count depends on the consultant's **canonical payment-period boundaries** (`CalculatePaymentPeriodDates`),
the **assignment history** lookup, and the **paid-holiday rule** — all server-side. Hardcoding `value="8"`
in the modal (the old approach) sidesteps this; a real default cannot.

## Decision

**The admin's input is a period total, not a per-day rate.**

- **Default = workable days × 8 h.** Workable days `N` is computed **server-side** as exactly
  `WeekdaySpread.GetWeekdayDates(...).Count` for the consultant/project/period — the *same* day-set the
  spread writes to. A small read-only preview endpoint returns `{ workableDays: N, suggestedTotal: N*8 }`;
  the upload UI calls it on open, prefills the total, and displays the day count ("N days") inline next
  to the input. Because the displayed count, the default, and the spread divisor are one server-computed
  `N`, they cannot diverge.

- **Spread = even split to the cent, remainder on the trailing days.** The total is split in integer
  cents: `baseCents = round(T×100) / N` per day, and the **last** `round(T×100) % N` days carry one extra
  cent each, so the movements sum to *exactly* the entered total `T`. `TimeFrom`/`TimeTo` are synthesized
  **per day** (so a remainder day gets its own window) instead of once for the whole period. In the default
  case `T = N × 8` this is a flat `8.00`/day with no remainder; days differ (by at most one cent) **only**
  when the admin overrides. Working in cents — rather than folding a single decimal remainder onto one day —
  is deliberate: that simpler scheme drives the last day **negative** when the per-day rounds up (e.g.
  `0.5 h / 14 days`), which would corrupt a pay figure. The cents split can never produce a negative day.

- **Holidays stay out of the admin's hands.** `N` reuses the existing paid-holiday rule with no new
  branch: paying-assignment holidays are excluded from `N` (the payment computation pays them separately
  via the auto **"Holidays"** movement, `NumHoursForHoliday`); non-paying holidays are included as normal
  work. The UI states this — "do not add holiday hours; if the project pays them they are added
  automatically later" — replacing the old "weekends and holidays are skipped" note.

## Considered options

- **Keep the per-day input, just show the computed total** (surface-only). Rejected: finance wants to
  *enter* the total they're filing, and overrides are framed as "± a few hours on the period," not "change
  the daily rate." Showing-only leaves the override in the wrong unit.

- **Even split, accept sub-cent drift** (`round(T/N, 2)` on every day, no remainder correction). Rejected:
  "I entered 150, it filed 149.99" is unacceptable for a pay figure. The last-day correction keeps
  *entered total = filed total* exact.

- **Constrain the override to multiples of `N`** (really a per-day rate in disguise). Rejected: defeats the
  purpose of a total input and is the surface-only option by another name.

- **Compute `N` client-side.** Rejected: the canonical period boundaries, assignment-history lookup, and
  paid-holiday rule are server-side; duplicating them in JS would let the displayed default drift from the
  filed total — the one invariant this ADR exists to protect.

## Consequences

- `UploadHoursOnBehalfVM` changes from `HoursPerDay` to a period total; the repository divides back to a
  per-day quantity and gains last-day remainder handling. `TimeFrom`/`TimeTo` synthesis moves inside the
  per-day loop.
- A new read-only preview endpoint (workable-day count + suggested total) is added, behind the same
  `AccessToManageTheBasicsOfPaymentSheets` policy as the upload.
- `WeekdaySpread` and its holiday test are unchanged — the consultant's *own* autofill still uses them; only
  the on-behalf path's input/spread shape changes.
- **Edge:** a large override (e.g. far more than `N × 24` hours) can drive the synthesized per-day window
  past 24h. The default and realistic overrides never approach this; if it matters, validate an upper
  bound on the total (or cap per-day at 24h) — deferred, noted here so it isn't a silent surprise.

### Resolved details

- **Server owns `N`.** The request carries the period **total only** — never a client-supplied `N` or
  per-day. Both the preview endpoint and the upload compute `N` from one shared
  `GetWorkableDays(consultant, project, periodDate)` helper, and the upload divides the total by *its own*
  recomputed `N`. The client value is a display/entry affordance, not a trust input.
- **`N = 0` is blocked.** A period with no workable days disables the upload (reusing the existing
  `weekdayDates.Count == 0` guard); no zero total is filed.
- **Degenerate days dropped.** When the total is smaller than one cent per day, the leading days come out
  at `0` (the non-zero cents land on the trailing days); the repository skips any `0`-quantity day so tiny
  totals never create 0-hour movements, and the filed total still equals the entered total exactly.
- **Input grid is `step="0.5"`, validation is `total > 0`.** Half-hour entry is a UI affordance; the
  backend does not enforce the 0.5 grid — `round(T/N, 2)` absorbs any value and the last-day remainder
  keeps *entered total = filed total* exact.
- **Window is display-only.** With a fractional per-day, the whole-minute `TimeFrom`/`TimeTo` window may
  not exactly equal `Quantity`; `Quantity` is the source of truth for pay (same rounding the code already
  did for the per-day input).
- **No hardcoded fallback.** If the preview call fails, the UI shows an error and keeps submit disabled
  until `N` is known — it does *not* silently fall back to 8/day (the behaviour this ADR removes).
</content>
</invoke>
