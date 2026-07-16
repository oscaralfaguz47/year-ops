# Weekly Pulse participation is role claims, never UserCategory

**Status:** accepted (2026-07-16)

## Context

Weekly Pulse access is gated on the `WeeklyPulseParticipate` claim: the nav link
(`Pages/Shared/_Layout.cshtml:87`) and the `Participate` policy on `DashboardController` /
`HistoryController` both check it. A user holds the claim when one of their **Identity roles**
carries it in `RoleClaims`. There is no `IsWeeklyPulseParticipant` column — participation *is*
role membership, derived at read time via `IsInRoleAsync`.

The consultant modal's "Weekly Pulse participant" checkbox was driven by something else entirely:
**`UserCategory`**. `updateWeeklyPulseCheckboxForCategory` forced the box to
`checked = true; disabled = true` whenever the category was `Administrative`, on the stated premise
that *"Administrative users already get Weekly Pulse access through the Admin/Master role claims."*

That premise is false. `UserCategory` (`Administrative` | `Consultant` | `External User`, an FK on
`ApplicationUser`) and Identity roles are **independent axes**. Nothing in the codebase adds a user
to the Admin role because their category is Administrative; category drives PTO rules, position
lists, and which roles the dropdown offers. An Administrative user whose role is neither Admin nor
Master therefore held **no Pulse claim while the UI displayed them as enrolled**.

This was not theoretical. On 2026-07-16, hours after the Pulse shipped to production, a Success
Manager (Administrative category, `Success Manager` role — a role with no Pulse claim at the time)
reported he could not see the Pulse while his checkbox showed ticked and greyed out. Production data
at that moment: **18 active Administrative users, only 7 with Pulse access.** The other 11 saw the
same misleading checkbox.

Two further defects compounded it:

1. **The disabled box posted `false`.** `createUpdateConsultantModal.js` read the payload as
   `!weeklyPulseCheckbox.disabled && weeklyPulseCheckbox.checked`, so the Administrative branch
   always sent `IsWeeklyPulseParticipant: false` — the UI showed checked, the wire said false.
2. **That silently stripped the role.** `ConsultantDetailRepository` diffs desired vs. actual and
   calls `RemoveFromRoleAsync` on mismatch. Saving an existing Weekly Pulse Participant as
   Administrative — for any unrelated edit, including the user editing their own record — removed
   the role while the modal still displayed them as enrolled.

A cosmetic lock became data loss because a control that could not express a value was still posting
one.

## Decision

**UserCategory never determines Pulse participation, in either direction.**

- The checkbox reflects exactly one fact — membership of the `Weekly Pulse Participant` role — and
  stays **enabled for every category**. Administrative users can now be enrolled and un-enrolled
  per-person like anyone else.
- The payload is read from `checked` alone. No control derives posted data from its own
  `disabled` state.
- "Their role already grants access" is a **fact about role claims**, so it is computed server-side
  (`CreateUpdateConsultantVM.RoleGrantsWeeklyPulseAccess`, resolved in
  `ConsultantDetailRepository.GetConsultantDataById` by joining the user's non-participant roles to
  `RoleClaims`) and rendered as a **read-only hint**. It never checks or disables the box.

The general rule this encodes: **when the UI wants to say something about permissions, it must read
permissions.** Category is a plausible-looking proxy, and proxies drift.

## Consequences

- The modal can no longer disagree with reality: what the box shows is what the role table says.
- Enrolment is uniformly opt-in and per-person; the 11 Administrative users who appeared enrolled
  now show their true (unenrolled) state, which is an accurate view, not a regression.
- Users whose job role grants Pulse (Admin, Master, and any role a Master granted it to) show an
  **unchecked** box plus the hint. That is correct — they are not in the participant role — but it
  is a two-source picture, and the hint is what keeps it legible.
- Granting the claim to a whole role (Admin Center → role permissions) remains the only way to
  enrol in bulk, and there is still **no way to un-enrol a single user whose role carries the
  claim** — only to strip the claim from the entire role. If per-person opt-out is ever needed for
  role-granted users, participation needs its own model rather than role claims doing double duty.
- Claims are baked into the auth cookie at sign-in, so any change here takes effect at the user's
  next sign-in (or within the 30-minute `SecurityStampValidator` refresh). The modal says so.

## Related

- `0002-team-as-new-org-entity.md` — Teams are leader-only; participation is not team membership.
- `0005-consultant-modal-silently-rewrites-roles.md` — the same modal, the same defect family, on the
  **role** dropdown rather than the Pulse checkbox. Hazard recorded, fix deferred; it emptied the
  `Master` role in production on the same day this ADR was written.
- **Known adjacent defect, not fixed here:** `RoleClaims.CreationDate` mixes time zones — the
  seeder writes `DateTime.UtcNow`, the Admin Center grant path writes Costa Rica local time
  (`UserRolesPermissionsController.cs:151`). Ordering by that column is meaningless; reconstructing
  the 2026-07-16 incident required cross-referencing row IDs against telemetry.
