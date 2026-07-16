// OCE Weekly Pulse — LOGIC PROTOTYPE (throwaway). Pure model + selectors.
//
// Question this answers: does the snapshot-vs-living split (ADR 0001) actually
// hold up when you drive it by hand? Specifically —
//   * snapshots blank each Week, living entities carry one identity across Weeks
//   * Review surfacing: Deferred goes quiet, Blocked stays loud, Pin overrides,
//     KPI in-scope gate hides tracked-only KPIs
//   * Meeting History as minutes: one living entity under every Week it was active in
//   * Readiness 3-state machine (KPI-only)
//   * Conversions preserve the source + carry a back-link
//
// This file is the bit worth lifting into Ripple later. The TUI (tui.js) is the
// throwaway shell. Selectors are pure; transitions mutate the passed state object.
//
// Source of truth for terminology: docs/oce-weekly-pulse/CONTEXT.md + ADR 0001.

// ---- enums ----------------------------------------------------------------
export const ISSUE_STATES = ['Open', 'Deferred', 'Solved']; // terminal: Solved
export const TODO_STATES = ['Open', 'Blocked', 'Done']; // terminal: Done
export const PRIORITIES = ['Low', 'Med', 'High', 'Critical'];
export const KPI_STATUS = ['Green', 'Yellow', 'Red'];
export const CHECKIN_TYPES = ['Win', 'Concern', 'Priority', 'Other'];
export const HEADLINE_TYPES = ['Highlight', 'Risk'];

// ---- id helper ------------------------------------------------------------
function nextId(state, kind, prefix) {
  state._seq[kind] = (state._seq[kind] || 0) + 1;
  return prefix + state._seq[kind];
}

// ---- week labels (Mon–Sun, illustrative) ----------------------------------
const BASE_MONDAY = new Date('2025-06-02T00:00:00'); // Week 1 Monday
function weekLabel(n) {
  const mon = new Date(BASE_MONDAY);
  mon.setDate(mon.getDate() + 7 * (n - 1));
  const sun = new Date(mon);
  sun.setDate(sun.getDate() + 6);
  const fmt = (d) => d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  return `Week ${n} (${fmt(mon)}–${fmt(sun)})`;
}
function weekMonday(n) {
  const mon = new Date(BASE_MONDAY);
  mon.setDate(mon.getDate() + 7 * (n - 1));
  return mon;
}

// The KPI History grouping unit. A Week belongs to the period of its Monday
// (CR time) — a Week that straddles a boundary is never split (see CONTEXT.md).
export const GRANULARITIES = ['month', 'quarter', 'year'];
export function periodOf(week, granularity) {
  const d = weekMonday(week);
  const y = d.getFullYear();
  const m = d.getMonth(); // 0–11
  if (granularity === 'year') return { key: `${y}`, label: `${y}` };
  if (granularity === 'quarter') {
    const q = Math.floor(m / 3) + 1;
    return { key: `${y}-Q${q}`, label: `Q${q} ${y}` };
  }
  return { key: `${y}-${String(m + 1).padStart(2, '0')}`, label: d.toLocaleDateString('en-US', { month: 'long', year: 'numeric' }) };
}

// KPI History: one KPI's weekly results across all Weeks, grouped by period.
// Display only — NO arithmetic roll-up of the (free-text) values. The per-period
// `tally` just counts statuses (judgment colors), to help read the trend by eye.
export function kpiHistory(s, kpiId, granularity = 'quarter') {
  const k = s.kpiDefs.find((x) => x.id === kpiId);
  if (!k) throw new Error(`no kpi ${kpiId}`);
  const groups = [];
  const byKey = {};
  for (let w = 1; w <= s.currentWeek; w++) {
    const r = kpiResult(s, kpiId, w);
    const status = r ? r.status : 'Missing';
    const period = periodOf(w, granularity);
    if (!byKey[period.key]) {
      byKey[period.key] = { key: period.key, label: period.label, rows: [], tally: { Green: 0, Yellow: 0, Red: 0, Missing: 0 } };
      groups.push(byKey[period.key]);
    }
    byKey[period.key].rows.push({ week: w, label: weekLabel(w), result: r || null, status });
    byKey[period.key].tally[status] += 1;
  }
  return { kpi: k, granularity, groups };
}

// ===========================================================================
// SELECTORS (pure) — these are the reusable core
// ===========================================================================

export const teamById = (s, id) => s.teams.find((t) => t.id === id);
export const personName = (s, id) => (s.persons.find((p) => p.id === id) || {}).name || '—';
export const teamsInOrder = (s) => [...s.teams].sort((a, b) => a.order - b.order);

export const kpiDefsForTeam = (s, teamId) => s.kpiDefs.filter((k) => k.teamId === teamId);
export const inScopeKpis = (s, teamId) =>
  kpiDefsForTeam(s, teamId).filter((k) => k.active && k.inScope);
export const kpiResult = (s, kpiId, week) =>
  s.kpiResults.find((r) => r.kpiId === kpiId && r.week === week);
export const checkinFor = (s, teamId, week) =>
  s.checkins.find((c) => c.teamId === teamId && c.week === week);
export const headlinesFor = (s, teamId, week) =>
  s.headlines.filter((h) => h.teamId === teamId && h.week === week);

// --- living-entity state derived from its event log (status is event-sourced)
// "as of week W" = the last status event with e.week <= W. This is what makes
// the minutes view honest: each Week shows the entity as it stood that week.
export function stateAsOf(entity, week) {
  let st = 'Open';
  for (const e of entity.events) {
    if (e.week <= week && (e.type === 'status' || e.type === 'created')) st = e.detail;
  }
  return st;
}
export const currentState = (entity, currentWeek) => stateAsOf(entity, currentWeek);
const isTerminal = (entity, week) =>
  entity.kind === 'issue' ? stateAsOf(entity, week) === 'Solved' : stateAsOf(entity, week) === 'Done';

export const issuesForTeam = (s, teamId) => s.issues.filter((i) => i.teamId === teamId);
export const todosForTeam = (s, teamId) => s.todos.filter((d) => d.teamId === teamId);
export const livingById = (s, id) =>
  s.issues.find((i) => i.id === id) || s.todos.find((d) => d.id === id);

// --- Readiness: 3-state machine, KPI-only ----------------------------------
export function readiness(s, teamId, week) {
  const scope = inScopeKpis(s, teamId);
  if (scope.length === 0) return 'Not configured';
  const allReported = scope.every((k) => {
    const r = kpiResult(s, k.id, week);
    return r && r.status; // a result with a status selected (Red counts)
  });
  return allReported ? 'Ready' : 'Not ready';
}

// --- Dashboard: the running input view for the current week -----------------
// Living lists keep Deferred issues (quiet but still on the open list) and all
// non-Done to-dos. Snapshots are whatever exists for this week.
export function dashboard(s) {
  const week = s.currentWeek;
  return teamsInOrder(s).map((team) => ({
    team,
    readiness: readiness(s, team.id, week),
    checkin: checkinFor(s, team.id, week) || null,
    kpis: kpiDefsForTeam(s, team.id).map((k) => ({ def: k, result: kpiResult(s, k.id, week) || null })),
    headlines: headlinesFor(s, team.id, week),
    issues: issuesForTeam(s, team.id)
      .filter((i) => stateAsOf(i, week) !== 'Solved') // Open + Deferred stay on the list
      .map((i) => ({ entity: i, state: stateAsOf(i, week) })),
    todos: todosForTeam(s, team.id)
      .filter((d) => stateAsOf(d, week) !== 'Done')
      .map((d) => ({ entity: d, state: stateAsOf(d, week) })),
  }));
}

// --- Review surfacing: the guided meeting view ------------------------------
// KPI:      in-scope gate; Red/Yellow/missing dominate, Green quiet.
// Issue:    union of (Open auto-surface) and (pinned). Deferred quiet unless pinned.
// To-Do:    every non-Done surfaces; Blocked is flagged loud.
// Headline: the week's news round — all of this week's headlines surface; Risk
//           is flagged loud, Highlight stays quiet. (Snapshot: blank next week.)
export function reviewSurfacing(s, week) {
  return teamsInOrder(s).map((team) => {
    const headlines = headlinesFor(s, team.id, week).map((h) => ({
      entity: h,
      loud: h.type === 'Risk',
    }));

    const kpis = inScopeKpis(s, team.id).map((k) => {
      const r = kpiResult(s, k.id, week);
      const status = r ? r.status : 'Missing';
      const loud = status !== 'Green'; // Red/Yellow/Missing dominate
      return { def: k, result: r || null, status, loud };
    });

    const issues = issuesForTeam(s, team.id)
      .map((i) => {
        const st = stateAsOf(i, week);
        const autoSurface = st === 'Open';
        const pinned = !!i.pin;
        return { entity: i, state: st, surfaced: autoSurface || pinned, pinned, autoSurface };
      })
      .filter((x) => x.surfaced && x.state !== 'Solved');

    const todos = todosForTeam(s, team.id)
      .map((d) => ({ entity: d, state: stateAsOf(d, week), blocked: stateAsOf(d, week) === 'Blocked' }))
      .filter((x) => x.state !== 'Done');

    return { team, headlines, kpis, issues, todos };
  });
}

// --- "Active in a Week" + Meeting History (minutes) -------------------------
// active-in-week = had a comment/status event that week OR surfaced in that
// week's Review. Derived, not stored. Surfacing uses the entity's state AS OF
// that week, so a since-Solved issue still appears under the weeks it was open.
function eventThisWeek(entity, week) {
  return entity.events.some(
    (e) => e.week === week && (e.type === 'status' || e.type === 'comment' || e.type === 'created')
  );
}
function surfacedInReviewThatWeek(entity, week) {
  const st = stateAsOf(entity, week);
  if (entity.kind === 'issue') return st === 'Open' || (!!entity.pin && st !== 'Solved');
  return st !== 'Done';
}
export function activeInWeek(entity, week) {
  if (week < entity.originWeek) return false;
  return eventThisWeek(entity, week) || surfacedInReviewThatWeek(entity, week);
}

export function meetingHistory(s, week) {
  const living = [...s.issues, ...s.todos].filter((e) => activeInWeek(e, week));
  return {
    week,
    label: weekLabel(week),
    // snapshots are strictly this-week
    checkins: s.checkins.filter((c) => c.week === week),
    headlines: s.headlines.filter((h) => h.week === week),
    kpiResults: s.kpiResults.filter((r) => r.week === week),
    summary: s.summaries.find((x) => x.week === week) || null,
    // living entities shown AS OF this week, with the events that landed this week
    living: living.map((e) => ({
      entity: e,
      state: stateAsOf(e, week),
      thisWeekEvents: e.events.filter((ev) => ev.week === week),
    })),
  };
}

// --- Weekly Summary: auto-assembled editable draft (plain derivation) -------
export function buildSummary(s, week) {
  const existing = s.summaries.find((x) => x.week === week);
  if (existing && existing.edited) return existing; // facilitator overrode it

  const solvedThisWeek = s.issues.filter((i) =>
    i.events.some((e) => e.week === week && e.type === 'status' && e.detail === 'Solved')
  );
  const inMeetingTodos = s.todos.filter((d) => d.originWeek === week && d.createdInMeeting);
  const riskHeadlines = s.headlines.filter((h) => h.week === week && h.type === 'Risk');
  const hotIssues = s.issues.filter(
    (i) => ['High', 'Critical'].includes(i.priority) && stateAsOf(i, week) === 'Open'
  );

  return {
    week,
    edited: false,
    decisions: solvedThisWeek.map((i) => i.title),
    actions: inMeetingTodos.map((d) => d.title),
    risks: [...riskHeadlines.map((h) => h.text), ...hotIssues.map((i) => `${i.title} (${i.priority})`)],
    text: `In ${weekLabel(week)}, the team solved ${solvedThisWeek.length} issue(s), committed ${inMeetingTodos.length} action(s), and is tracking ${riskHeadlines.length + hotIssues.length} risk(s).`,
  };
}

// ===========================================================================
// TRANSITIONS (mutate state) — the throwaway-shell drives these
// ===========================================================================

export function advanceWeek(s) {
  // Lazy creation of the next Week. Snapshots are simply absent for it (blank).
  // Living entities are NOT touched — same identity carries forward. ADR 0001.
  s.currentWeek += 1;
  if (!s.weeks.find((w) => w.n === s.currentWeek)) {
    s.weeks.push({ n: s.currentWeek, label: weekLabel(s.currentWeek) });
  }
  return s;
}

export function upsertCheckin(s, teamId, type, note) {
  let c = checkinFor(s, teamId, s.currentWeek);
  if (c) {
    c.type = type;
    c.note = note;
  } else {
    c = {
      id: nextId(s, 'checkin', 'C'),
      teamId,
      week: s.currentWeek,
      type,
      note,
      ownerId: teamById(s, teamId).leaderId,
    };
    s.checkins.push(c);
  }
  return c;
}

// Create a KPI definition (a structural change — the UI guards it with a
// confirm; see "Guarded mutation" in CONTEXT.md). Owner defaults to the Team
// Leader. New KPIs start live + in-scope so the Team can be taken to Readiness.
export function addKpiDef(s, teamId, name, target, { active = true, inScope = true } = {}) {
  const k = { id: nextId(s, 'kpi', 'K'), teamId, name, ownerId: teamById(s, teamId).leaderId, target, active, inScope };
  s.kpiDefs.push(k);
  return k;
}

// Edit a KPI definition's name and/or target (also a guarded structural change).
// Only the definition fields are touched — never the weekly KPI results.
export function editKpiDef(s, kpiId, { name, target } = {}) {
  const k = s.kpiDefs.find((x) => x.id === kpiId);
  if (!k) throw new Error(`no kpi ${kpiId}`);
  if (name != null && name !== '') k.name = name;
  if (target != null) k.target = target;
  return k;
}

export function setKpiResult(s, kpiId, status, actual, notes = '') {
  let r = kpiResult(s, kpiId, s.currentWeek);
  if (r) {
    r.status = status;
    r.actual = actual;
    if (notes) r.notes = notes;
  } else {
    r = { id: nextId(s, 'kpiResult', 'R'), kpiId, week: s.currentWeek, status, actual, notes };
    s.kpiResults.push(r);
  }
  return r;
}

export function addHeadline(s, teamId, type, text) {
  const h = { id: nextId(s, 'headline', 'H'), teamId, week: s.currentWeek, type, text };
  s.headlines.push(h);
  return h;
}

export function addIssue(s, teamId, title, priority = 'Med', origin = null) {
  const i = {
    id: nextId(s, 'issue', 'I'),
    kind: 'issue',
    teamId,
    ownerId: teamById(s, teamId).leaderId,
    title,
    priority,
    originWeek: s.currentWeek,
    pin: false,
    origin,
    events: [{ week: s.currentWeek, type: 'created', detail: 'Open' }],
  };
  s.issues.push(i);
  return i;
}

export function addTodo(s, teamId, title, createdInMeeting = false, origin = null) {
  const d = {
    id: nextId(s, 'todo', 'D'),
    kind: 'todo',
    teamId,
    ownerId: teamById(s, teamId).leaderId,
    title,
    due: '',
    originWeek: s.currentWeek,
    createdInMeeting,
    origin,
    events: [{ week: s.currentWeek, type: 'created', detail: 'Open' }],
  };
  s.todos.push(d);
  return d;
}

export function setLivingState(s, id, newState) {
  const e = livingById(s, id);
  if (!e) throw new Error(`no living entity ${id}`);
  const legal = e.kind === 'issue' ? ISSUE_STATES : TODO_STATES;
  if (!legal.includes(newState)) throw new Error(`illegal state ${newState} for ${e.kind}`);
  e.events.push({ week: s.currentWeek, type: 'status', detail: newState });
  return e;
}

export function addComment(s, id, text) {
  const e = livingById(s, id);
  if (!e) throw new Error(`no living entity ${id}`);
  e.events.push({ week: s.currentWeek, type: 'comment', detail: text });
  return e;
}

export function togglePin(s, id) {
  // Pin is a Deferred-issue override: it un-parks a Deferred issue back into the
  // Review. Open issues surface on their own and Solved ones are done, so pinning
  // only does real work on a Deferred issue — that's the only state it's offered
  // in. Headlines are not pinnable — they all surface in the news round.
  const e = livingById(s, id);
  if (!e || e.kind !== 'issue') throw new Error(`no pinnable issue ${id}`);
  const st = stateAsOf(e, s.currentWeek);
  if (st !== 'Deferred') throw new Error(`only Deferred issues are pinnable (${id} is ${st})`);
  e.pin = !e.pin;
  return e;
}

export function setPriority(s, id, priority) {
  const e = livingById(s, id);
  if (!e || e.kind !== 'issue') throw new Error(`no issue ${id}`);
  e.priority = priority;
  return e;
}

// --- Conversions: always additive, source preserved, back-link carried ------
export function convertCheckinToIssue(s, checkinId) {
  const c = s.checkins.find((x) => x.id === checkinId);
  if (!c) throw new Error(`no check-in ${checkinId}`);
  return addIssue(s, c.teamId, `[from check-in] ${c.note}`, 'Med', { type: 'checkin', id: c.id });
}
export function convertHeadlineToIssue(s, headlineId) {
  const h = s.headlines.find((x) => x.id === headlineId);
  if (!h) throw new Error(`no headline ${headlineId}`);
  const pri = h.type === 'Risk' ? 'High' : 'Med';
  return addIssue(s, h.teamId, `[from headline] ${h.text}`, pri, { type: 'headline', id: h.id });
}
export function convertIssueToTodo(s, issueId, createdInMeeting = false) {
  const i = s.issues.find((x) => x.id === issueId);
  if (!i) throw new Error(`no issue ${issueId}`);
  return addTodo(s, i.teamId, `[from issue] ${i.title}`, createdInMeeting, { type: 'issue', id: i.id });
}

export function saveSummary(s, summary) {
  summary.edited = true;
  const idx = s.summaries.findIndex((x) => x.week === summary.week);
  if (idx >= 0) s.summaries[idx] = summary;
  else s.summaries.push(summary);
  return summary;
}

// ===========================================================================
// SEED — confirmed roster + enough data to show every contrast on first run
// ===========================================================================
export function createSeedState() {
  const s = {
    currentWeek: 1,
    weeks: [{ n: 1, label: weekLabel(1) }],
    persons: [],
    teams: [],
    kpiDefs: [],
    kpiResults: [],
    checkins: [],
    headlines: [],
    issues: [],
    todos: [],
    summaries: [],
    _seq: {},
  };

  const P = (name) => {
    const p = { id: nextId(s, 'person', 'P'), name };
    s.persons.push(p);
    return p;
  };
  const eder = P('Eder Rodriguez');
  const david = P('David Barrios');
  const pri = P('Pri Zamora');
  const andrey = P('Andrey Espinoza');
  const laura = P('Laura Paniagua');
  const andy = P('Andy Monge');
  const oscar = P('Oscar Alfaro');

  let order = 0;
  const T = (name, leaderId) => {
    const t = { id: nextId(s, 'team', 'T'), name, leaderId, order: order++ };
    s.teams.push(t);
    return t;
  };
  const leadership = T('Leadership', eder.id);
  const sales = T('Sales', david.id);
  const marketing = T('Marketing', david.id); // David leads two teams
  const ops = T('Operations', pri.id);
  T('Recruiting', andrey.id);
  T('People & Culture', laura.id);
  T('Success Management', andy.id);
  T('Finance', oscar.id);

  const KPI = (teamId, name, ownerId, target, { active = true, inScope = true } = {}) => {
    const k = { id: nextId(s, 'kpi', 'K'), teamId, name, ownerId, target, active, inScope };
    s.kpiDefs.push(k);
    return k;
  };
  // Sales: 2 in-scope; one reported (Green), one missing -> Not ready
  const salesPipeline = KPI(sales.id, 'Pipeline value', david.id, '$50k');
  const demosBooked = KPI(sales.id, 'Demos booked', eder.id, '≥ 10'); // owner Eder = cross-team owner, grouped under Sales
  // Marketing: one in-scope + one tracked-only (out of scope) to show the scope gate
  const mql = KPI(marketing.id, 'MQLs', david.id, '≥ 40');
  KPI(marketing.id, 'Blog drafts (tracked only)', david.id, '≥ 2', { inScope: false });
  // Operations: 2 in-scope, both reported (one Red) -> Ready, Red surfaces loud
  const opsUptime = KPI(ops.id, 'Delivery on-time %', pri.id, '≥ 95%');
  const opsBugs = KPI(ops.id, 'Escaped defects', pri.id, '< 3');
  // Leadership: a retired KPI (active=false) — present but doesn't gate readiness
  KPI(leadership.id, 'Cash runway (retired)', eder.id, '> 12mo', { active: false });
  const nps = KPI(leadership.id, 'Company NPS', eder.id, '≥ 50');

  // Living entities, originating in Week 1 (they persist to the current Week)
  s.currentWeek = 1;
  const pricing = addIssue(s, sales.id, 'Pricing page confuses enterprise leads', 'High');
  const officeMove = addIssue(s, ops.id, 'Office move logistics', 'Med');
  setLivingState(s, officeMove.id, 'Deferred'); // quiet on Review, still on Dashboard
  const parkedButPinned = addIssue(s, marketing.id, 'Rename the newsletter', 'Low');
  setLivingState(s, parkedButPinned.id, 'Deferred'); // parked -> normally quiet on Review
  togglePin(s, parkedButPinned.id); // ...but pinned, so it surfaces anyway (pin overrides Deferred)
  const legal = addTodo(s, sales.id, 'Get legal sign-off on new contract');
  setLivingState(s, legal.id, 'Blocked'); // loud, keeps surfacing
  addTodo(s, ops.id, 'Publish new on-call rota');

  // --- A multi-Week KPI result history so KPI History has periods to show -----
  // 14 Weeks span Jun–Sep 2025 → Q2 (June) + Q3 (Jul/Aug/Sep). The CURRENT Week
  // is Week 14; its results reproduce the Week-1 readiness demo (some missing).
  // Statuses are seed judgments (no arithmetic in the product — see CONTEXT.md).
  const CUR = 14;
  const series = {
    [salesPipeline.id]: ['$41k', '$44k', '$48k', '$52k', '$55k', '$58k', '$54k', '$60k', '$63k', '$66k', '$61k', '$64k', '$68k', '$62k'],
    [opsUptime.id]:     ['90%', '92%', '94%', '96%', '95%', '93%', '91%', '94%', '96%', '97%', '95%', '94%', '92%', '88%'],
    [mql.id]:           ['31', '34', '38', '42', '45', '40', '37', '41', '44', '47', '43', '39', '36', '34'],
    [opsBugs.id]:       ['4', '3', '2', '2', '1', '2', '3', '1', '0', '1', '2', '1', '2', '1'],
    [demosBooked.id]:   ['7', '8', '9', '11', '12', '10', '9', '12', '13', '14', '11', '10', '12', null], // missing this Week
    [nps.id]:           ['44', '46', '48', '51', '53', '50', '49', '52', '54', '56', '53', '51', '49', null], // missing this Week
  };
  const num = (v) => parseFloat(String(v).replace(/[^0-9.-]/g, ''));
  const statusFns = {
    [salesPipeline.id]: (v) => (num(v) >= 50 ? 'Green' : num(v) >= 45 ? 'Yellow' : 'Red'),
    [opsUptime.id]: (v) => (num(v) >= 95 ? 'Green' : num(v) >= 93 ? 'Yellow' : 'Red'),
    [mql.id]: (v) => (num(v) >= 40 ? 'Green' : num(v) >= 35 ? 'Yellow' : 'Red'),
    [opsBugs.id]: (v) => (num(v) < 3 ? 'Green' : num(v) === 3 ? 'Yellow' : 'Red'),
    [demosBooked.id]: (v) => (num(v) >= 10 ? 'Green' : num(v) >= 8 ? 'Yellow' : 'Red'),
    [nps.id]: (v) => (num(v) >= 50 ? 'Green' : num(v) >= 45 ? 'Yellow' : 'Red'),
  };
  for (let w = 1; w <= CUR; w++) {
    s.currentWeek = w;
    if (!s.weeks.find((x) => x.n === w)) s.weeks.push({ n: w, label: weekLabel(w) });
    for (const kid of Object.keys(series)) {
      const v = series[kid][w - 1];
      if (v != null) setKpiResult(s, kid, statusFns[kid](v), v);
    }
  }
  s.currentWeek = CUR;

  // Current-Week (Week 14) snapshots — the live Dashboard/Review demo
  upsertCheckin(s, sales.id, 'Win', 'Closed the Acme deal');
  upsertCheckin(s, ops.id, 'Concern', 'Two senior devs out sick');
  addHeadline(s, ops.id, 'Risk', 'Vendor SLA slipping, may affect Q3');
  addHeadline(s, sales.id, 'Highlight', 'Hit 120% of pipeline goal');

  return s;
}

export { weekLabel };
