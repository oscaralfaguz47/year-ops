# OCE Weekly Pulse — logic prototype

**Throwaway.** Validates the data/state model in `../CONTEXT.md` + `../../adr/0001-snapshot-vs-living-entities.md` *before* any Ripple build. No DB, no auth, in-memory only.

## Run

**Browser (clickable):**
```
cd docs/oce-weekly-pulse/prototype
npm start          # or: node serve.js
# open http://localhost:5179
```
Four tabs — Dashboard / Review / History / Summary — over one dataset. "Advance week ▶" is the time machine; "Reset" reseeds. Action buttons sit inline on each item (state, pin, comment, convert, log). Text inputs use browser prompts.

**Terminal (same logic, keyboard-driven):**
```
npm run tui        # or: node tui.js
```
Line-based commands (type + Enter); cheat-sheet is the footer.

Both shells import the **same `model.js`** — the browser and terminal are interchangeable views over one logic core.

## The question it answers

Does the **snapshot-vs-living split** hold up when you drive it by hand? The things easy to get wrong on paper:

1. **Living vs snapshot end-to-end** — a Week starts blank for snapshots (check-ins, KPI results, headlines) while Issues/To-Dos carry one identity forward. *Try:* note a Sales issue, press `w` twice → it's still on W3's Dashboard; the check-in is gone.
2. **Review surfacing** — Deferred goes quiet, Blocked stays loud, Pin overrides, in-scope gate hides tracked-only KPIs, Green KPIs stay quiet. *Try:* compare lens `1` (Dashboard) vs `2` (Review) on the seed data — `I2` (Deferred) shows on Dashboard, vanishes from Review; `I3` (Low, pinned) appears anyway; `D1` (Blocked) is loud in both.
3. **Minutes history** — one living entity under every Week it was active in, shown *as of* that week. *Try:* `cm I1 still open` in W1, `w`, `st I1 solved` in W2, then lens `3` + `<`/`>` — `I1` appears under both weeks with its per-week progression; `show I1` shows the event log and the weeks it's active in.
4. **Readiness** 3-state machine (KPI-only). *Try:* Sales seeds as **Not ready** (Demos booked has no result) → `res K2 g 12` → flips to **Ready**. `active K2` on a lone KPI shows **Not configured** behavior.
5. **Conversions** preserve the source + carry a back-link. *Try:* `cv.hl H1` (Ops risk headline → issue) then `show` the new issue — source headline still in W1, issue shows `←headline H1`.

## Seed contrasts (Week 1, focus = Sales)

- Sales: Win check-in, Pipeline KPI Green, **Demos booked missing → Not ready**, issue `I1` (High, Open), to-do `D1` (Blocked).
- Operations: 2 KPIs reported (one **Red**, loud) → Ready, Risk headline `H1`, deferred issue `I2`.
- Marketing: one in-scope KPI + one **tracked-only** KPI (scope gate), low-priority **pinned** issue `I3`.
- Leadership: a **retired** KPI (doesn't gate readiness).
- David Barrios leads both Sales & Marketing; the Demos KPI is owned by Eder (cross-team owner, still grouped under Sales).

## Verdict (fill in after driving it)

> _What did clicking through teach us? Did the living/snapshot split feel right, or did something read as duplication / surprise? Capture here, then fold decisions into CONTEXT.md and delete this folder._

- Living vs snapshot:
- Surfacing rules:
- Minutes history:
- Readiness:
- Conversions:

## Known prototype simplifications (intentional)

- **Pin is current-state only**, not historized — Review-for-a-past-week uses today's pin flag. Status *is* event-sourced, so the load-bearing axis (Open/Deferred/Solved across weeks) is faithful.
- No auto-reactivation of Deferred on a revisit Week (manual `st I2 open` only).
- No persistence; `seed` is the only "reset".
