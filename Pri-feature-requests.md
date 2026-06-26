# Ripple — Feature Requests from Priscila Call

Extracted from the call transcript with Priscila Zamora Quirós. UI/UX notes from the call are included where discussed.

---

> **Feature 1 — Link non-hours payments to a consultant on the payment sheet — SHIPPED.** Delivered as Non-Hours Payments M1 (interviews, issue #41) + M2 (debits/credits + reimbursements, issue #42); PRD issue #40. Removed from the backlog. Numbering below is preserved as stable IDs.

## 2. Manually change a payment record's status to Paid

Today she can edit or reject a record but has no way to report that it's already been paid — a problem when someone leaves mid-period and is paid early outside the normal cycle.

**Required behavior:** an action to manually set a record's status to Paid. For consultants in a normal payment sheet the status updates automatically; for those who aren't, it stays pending until changed manually.

**UI/UX from the call:**
- A button in the same family as the existing actions ("review for payment" / "change status" / "approved"), specifically a "change to Paid" control.
- Records not yet covered by development stay in their current state until the manual status change exists.

---

## 3. Manual hours upload on behalf of a consultant (per pay period) — SHIPPED

> **SHIPPED.** Delivered as the MHU entry point (a payment-sheet row button that opens an upload-on-behalf modal) via PR #46 (`feature/manual-hours-upload` → demo), with the project-less "remove for this period" cleanup in PR #47. Built exactly as the resolved design below. Numbering preserved as a stable ID.

There's no place in Ripple to manually enter hours for someone else; the only existing route adds hours straight into the database as if self-reported. Needed when a consultant can't submit themselves — Priscila's upcoming ~4-month leave is the driving case, and the same applies to anyone on extended/maternity leave or vacation.

**Required behavior:** an admin-facing manual hours submission. The admin selects the consultant, selects the project, enters the number of hours, and confirms. After submission it behaves exactly like a normal timesheet submission and continues through the standard Ripple flow (review for payment → approve → pay). Scoped **per quincena (pay period)**, not a single lump for the whole absence, so early returns or extended absences are handled period by period. Agreed to start per-quincena and phase further later.

**UI/UX from the call:**
- Placed under **actions**, as a button like "manual hours upload / submission."
- Required inputs: choose consultant, choose project (she stressed project selection as very important), enter hours, then a confirmation step.
- Once submitted it must surface like a normal submission — the three-dots state with "review for payment" becoming available — so downstream processing is identical.

**Resolved design (see `CONTEXT.md` → Manual Hours Upload, and `docs/adr/0002-manual-hours-upload-as-admin-autofill.md`):**
- Modelled as **autofill performed by an admin on behalf of an absent consultant**: reuse the existing autofill spread + the submit step, landing at **"Waiting to be approved"** → normal review → approve → pay. **Never auto-approved** (do not reuse the `RemoveProjectConsultantInPeriod` "Approved" shortcut).
- **Requires an active assignment** to the chosen project (reuses normal submission validation). Mirror image of Feature 1's project-less rows — unrelated to the Payment Anchor.
- **No-tracking-tool projects only.** Tracking-tool projects are out of scope (their submit needs consultant-supplied evidence screenshots an admin can't produce) → stay a manual/operator case. No evidence gate is bypassed.
- **Collision guard reused unchanged** (`ValidateSubmission`): an already-submitted non-rejected period is blocked; a rejected one is re-submitted; drafts are overwritten.
- **Actor vs. subject split:** movements + submission key to the **subject** consultant (incl. their `PaymentPeriod`); the **acting admin** is recorded via `UserIdLastUpdatedBy` + a `ReportingMyTimeComments` "uploaded on behalf of" marker.
- **Notifications:** fire the internal "new submission to review" email; "you owe hours" reminders self-suppress once a submission exists (one possible reminder before upload is accepted; no leave-aware suppression built).
- **Placement/auth:** new action in `Finances/PaymentSheetsController` behind the existing `AccessToManageTheBasicsOfPaymentSheets` policy — no new policy/claim.

---

> **Time-off card group — Features 4 + 5 + 6 ship together (one branch off `demo`).** Two *independent* concerns that happen to touch the **same card surface** (`renderBalancesCard()` in `timeOff.js` + `TimeOff/Index.cshtml`), which is the real reason to keep them on one branch — they would otherwise conflict on the same view:
> - **VTO track (Features 4 + 5)** — Voluntary Time Off, the 1-day/year line (`TimeOffType == "VTO"`). Feature 5 carves it into its own card; Feature 4 validates available VTO on submit.
> - **PTO track (Feature 6)** — the vacation balance. Collapse the carried-over (from Vacation Tracker) vs accrued-month-to-month split into one figure. This is the **Administrative-PTO** display only (`InitialAdminPtoBalance` + monthly accrual); consultants never see the split.
>
> Earlier note retracted: VTO and PTO are **different balances** (VTO is a hardcoded `1 − used`; PTO carries-over + accrues), so 4 does **not** validate against 6's number. No cross-dependency — sequence is free; split into VTO PR (4+5) and PTO PR (6) if size demands. Feature 2 stays separate (payment-sheet, sibling to Feature 3). "polisis/pitio" in the transcript = the existing **"Policies and PTO"** card.

## 4. VTO availability validation on submit

When a user selects a VTO day, "available" can show 0, yet submitting still creates a "waiting to be approved" request.

**Required behavior:** validate the user's available VTO balance on submit. If the balance is 0 / insufficient, block submission and return an error rather than creating the request. If sufficient, process normally. Applies to **all users**, not just the admin team.

---

## 5. Separate VTO card, visible to everyone

The VTO functionality should live in its own card so it can be shown universally — the polisis/pitio card is not shown to everyone, so keeping VTO separate makes it easy to display to all users.

**UI/UX from the call:**
- New VTO card positioned **below** the current card, with the polisis card **last**.

---

## 6. VTO/PTO card — single consolidated balance (stop separating carried-over vs. accrued)

Right now the card splits the balance into two lines: what was carried over from Vacation Tracker versus what's been accrued month-to-month in Ripple. This is confusing — combined it shows e.g. 6.5 days, but the split makes the math hard to follow, and nobody can reasonably reconstruct their full history (days accrued since joining minus all vacation taken) from this card.

**Decided behavior (Priscila + Andy's preference, which you agreed to):**
- **Do not separate** the carried-over balance from the accrued-in-Ripple balance. Collapse them into a **single line / single "current balance"** number (e.g. "you have 6.5"). The balance = what came from Vacation Tracker + what's accrued month-to-month, as one figure.
- The current balance **edits month to month automatically**: +1 each month (accrual is one day per month), minus whatever is used.
- **Keep showing "used"** — Priscila sees value in people knowing how much they've spent. So: current balance and used are shown; total derives from them. What's removed is the carried-over-vs-accrued *split*, not the used count.
- Rationale she gave: everyone already has their own access, history, and request log in Vacation Tracker, so the per-source breakdown doesn't need to live in Ripple.

**UI/UX from the call:**
- Pending fix so the value doesn't wrap onto two lines, and so the row doesn't repeat "days" — "days" should appear only once, at the top, to avoid a cluttered look.
- The accrual has a defined start date ("desde esta fecha... donde empezamos a contar mes a mes"), which is the anchor for month-to-month counting.

---

## Open flags

- Priscila twice mentioned a third/other item she couldn't recall ("había algo más, pero no me acuerdo qué era") — never stated, so not captured. Worth pinging her.
- The **Weekly Pulse task tracker placement** came up (put the advanced task tracker under the Dashboard module as a tab — "main dashboard" / "nearshore dashboard"), but it's being treated as a separate project and is intentionally excluded here.
