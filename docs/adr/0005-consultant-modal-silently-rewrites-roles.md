# The consultant modal silently rewrites roles — hazard documented, fix deferred

**Status:** accepted (2026-07-16) — **hazard recorded, fix deliberately deferred**

> This ADR does not change code. It records a live production hazard, the incident that proved it,
> and the reasoning for not fixing it in the same change as
> `0004-pulse-participation-is-role-claims-not-user-category.md`. Read this before touching
> `fillRolesForSelect`, `GetAllRolesListForSelect`, or the role-diffing block in
> `ConsultantDetailRepository`.

## Context

Saving a consultant through the create/update modal does not only save the fields the editor
touched. `ConsultantDetailRepository` (~lines 232-257) diffs the posted role against the user's
actual roles and **acts on any mismatch**:

```csharp
var actualJobRole = actualUserRole.FirstOrDefault(r => r != SD.Role_User_Weekly_Pulse_Participant);
if (actualJobRole != consultantData.UserRole)
{
    _userManager.RemoveFromRoleAsync(existingUser, actualJobRole)...
    _userManager.AddToRoleAsync(existingUser, consultantData.UserRole)...
}
```

So whatever the role dropdown posts *is* the new role — including when the dropdown never had a way
to represent the old one. Two mechanisms make that reachable:

**1. The dropdown hides `Master` from non-Masters.** `GetAllRolesListForSelect`
(`UserRolesPermissionsController.cs:113-128`) filters the option list:

```csharp
var missingRole = "";
if (!User.IsInRole("Master")) { missingRole = "Master"; }
var roles = _roleManager.Roles.ToList().Where(x => x.Name != missingRole).OrderBy(x => x.Name);
```

The intent is sound — an Admin should not be able to mint Masters. But `fillRolesForSelect`
(`createUpdateConsultantModal.js:288-315`) then does `selectElement.value = userRole` on the edit
path. When an **Admin opens a Master's record**, `userRole` is `"Master"`, the option was just
filtered out, the assignment silently no-ops (`selectedIndex` → -1, the control renders blank), and
saving for any unrelated reason posts a role that is not the user's actual one. The server removes
`Master`. **A guard against granting Master became a mechanism for silently revoking it.**

**2. Changing the category resets the role.** The category select is wired as
`onchange="selectCategory(this.value)"` (`_CreateUpdateConsultantModalPartialView.cshtml:70`) —
one argument, against a four-parameter signature
(`selectCategory(selectedValue, selectedOptions, isEditingConsultant, userRole)`). `isEditingConsultant`
arrives `undefined`, so `fillRolesForSelect` takes the `!isEditingConsultant` branch and forces
`selectElement.value = 'Simple'`. Toggling the category on an existing user therefore silently
re-targets their role to `Simple` — a role that carries only `ReportingMyTime`.

This is the same defect family as ADR 0004: **a control that cannot represent the current value is
still allowed to post one, and the server treats that as an intentional change.**

## The incident that proved it (2026-07-16)

- ~15:10 — `oscar.alfaro@oceanscode.com` held the `Master` role; Master had exactly one member.
- 15:37:18 — his consultant record was saved **by himself** (`UserLastUpdatedBy` = his own id).
- After that save — his roles were `['Admin']`, and **the `Master` role had zero members.**

That emptied role is not cosmetic. `DbInitializer` (lines 1443-1531) gates startup on it: with no
Master member it reads `MasterUserEmailENV` (prod: `oscar.alfaro@oceanscode.com`), finds that user
already exists, and throws `InvalidOperationException("User exists but is not assigned to
Role_User_Master.")`. In `Program.cs`, `await SeedDatabaseAsync(app)` (line 240) runs **before**
`await app.RunAsync()` (line 265), and the `catch` (line 268) only logs. **The next restart of
production would not have booted** — silently, as a log line rather than a crash. Prod stayed up
only because the running process was never recycled.

Resolved the same day by restoring Master membership (Oscar, plus `dania.chavarria@` as a second
member so a single bad save cannot empty the role again). Startup gate re-validated as passing.

**What is proven vs. not.** Proven: the `missingRole` filter; the unconditional role-diff/replace;
the one-argument `onchange`; Oscar's Master→Admin transition across his own 15:37 save; the empty
role; the startup gate's behavior. **Not proven:** the exact trigger at 15:37 — Oscar was still a
Master at that moment, so his own dropdown *should* have offered the option. The hazard below stands
regardless of which path fired.

## Decision

**Record the hazard now; do not fix it in this change.**

The Pulse work (ADR 0004) is scoped to participation. Fixing the role dropdown means touching role
assignment for every consultant, every category, and every editor — a materially larger blast radius
than the Pulse checkbox, on the same screen that just caused an outage-in-waiting. Shipping both in
one branch would make the Pulse fix hard to review and hard to revert independently. The trigger is
also not yet nailed down, and a fix aimed at the wrong mechanism is worse than a documented hazard.

## Guard rails until it is fixed

- **Do not save a Master's consultant record while signed in as a non-Master.** The role dropdown
  cannot represent `Master`, and saving will remove it.
- **Do not toggle User Category on an existing user** unless you also re-select their role before
  saving; the toggle silently re-targets it to `Simple`.
- **Keep at least two members in the `Master` role.** The startup gate needs one; the second is the
  margin that keeps a single bad save from becoming a failed deploy.
- **Check `Master` has members before any deploy to `main`** — a deploy is exactly the restart that
  trips the gate.

## When fixed, the shape should be

- If `userRole` is not among the fetched options, render it as a **disabled, selected** option so the
  value displays and round-trips intact instead of blanking.
- **Reject** an empty/unknown posted role server-side rather than treating it as "remove the current
  one." The server check is the one that matters — it cannot be bypassed by a client that gets it
  wrong.
- Pass the full argument list from the category `onchange`, or drop the reset entirely and let the
  role selection stand on its own.
- Consider whether `DbInitializer` should **repair** a missing Master (re-assign the env-var user)
  rather than throw and take the app down — a self-healing seed would have made the incident a
  non-event.

## Related

- `0004-pulse-participation-is-role-claims-not-user-category.md` — same modal, same defect family
  (a disabled checkbox posting `false` and silently stripping the participant role).
- **Adjacent, also unfixed:** `RoleClaims.CreationDate` mixes time zones — the seeder writes
  `DateTime.UtcNow`, the Admin Center grant path writes Costa Rica local
  (`UserRolesPermissionsController.cs:151`). Ordering by it is meaningless; reconstructing this
  incident required cross-referencing row IDs against Application Insights telemetry.
