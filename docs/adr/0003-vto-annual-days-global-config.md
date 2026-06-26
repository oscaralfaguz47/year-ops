# VTO annual days is a global config value, stored on AdminPtoConfiguration

**Status:** accepted

## Context

Feature 5 adds a VTO (Voluntary Time Off) card that displays the consultant's voluntary-day balance
and the "X days per year" entitlement, and Priscila asked that the per-year number come "from the
database" so it can change without a code deploy. Today the entitlement is a **hardcoded `1`** in the
balance math (`TimeOffRequestRepository.GetBalancesAsync`: `result.VtoAvailable = 1 - vtoUsedAndPending`),
and the Feature 4 submit guard validates against that same literal. There is no DB field for it
anywhere.

VTO is a **company-wide perk** — the same allowance for everyone — not a per-person amount. The
codebase already has the right precedent for a company-wide time-off number: `AdminPtoConfiguration`,
a **single-row global table** (`AnnualPaidDays`, `EffectiveDate`) seeded in `DbInitializer` with **no
admin edit screen**; it is changed via migration/DB and read by the time-off repository.

## Decision

Add a **`VtoAnnualDays`** column to the existing single-row **`AdminPtoConfiguration`** table, seeded
to **`1`** (preserving today's behavior). Make it the single source of truth read by **all three**
sites: the VTO card display, the balance calc at `TimeOffRequestRepository:54` (replacing the literal
`1`), and the Feature 4 submit validation — so the displayed entitlement, the available balance, and
the hard block can never drift apart.

Follow the sibling config's operational model exactly: **no admin UI**, seeded value, changed via
migration when needed. VTO stays **global** (not per-consultant) because it is a uniform perk.

## Considered options

- **A new `VtoConfiguration` (or rename to `TimeOffConfiguration`) table** — cleaner semantics, since
  VTO is not "admin PTO". Rejected for now: a new table or a rename touches the migration history and
  model snapshot for a cosmetic gain, when the time-off repository already loads the
  `AdminPtoConfiguration` row in the same file. The naming wart (a VTO field living on an "AdminPto"
  table) is the reason this ADR exists — see Consequences.
- **A per-consultant `VtoAnnualDays` on `ConsultantDetail`** (mirroring `AnnualPaidTimeOffDays`) —
  rejected: VTO is a company-wide entitlement, identical for everyone; per-consultant storage invites
  drift and an editing surface nobody asked for.
- **Build an admin screen to edit it now** — rejected: its sibling `AdminPtoConfiguration` has no
  screen; adding one for VTO would invent a pattern the codebase doesn't have. Seed it; add an editor
  later only if asked.

## Consequences

- A future reader will find a VTO field on a table named `AdminPtoConfiguration` and reasonably
  wonder why — this ADR is the answer. The table is effectively becoming a global *time-off* config;
  if a third such field ever lands, that's the trigger to rename it to `TimeOffConfiguration`.
- Changing the company VTO allowance is a DB/migration edit, not a UI action — acceptable given how
  rarely it changes and that the admin-PTO config works the same way.
- Because the literal `1` is replaced by config in the calc, Feature 4's hard block automatically
  validates against the configured number; the card, balance, and guard share one value by
  construction.
