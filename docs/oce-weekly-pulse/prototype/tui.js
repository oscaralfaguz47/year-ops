// OCE Weekly Pulse — LOGIC PROTOTYPE (throwaway TUI shell). Drive model.js by hand.
// Run:  node tui.js   (or: npm start)
//
// This shell is disposable. The reusable logic lives in model.js. Commands are
// line-based (type + Enter) so notes/titles can carry spaces. `?` lists commands.

import readline from 'node:readline';
import * as M from './model.js';

// ---- ansi ----
const B = (x) => `\x1b[1m${x}\x1b[0m`;
const D = (x) => `\x1b[2m${x}\x1b[0m`;
const G = (x) => `\x1b[32m${x}\x1b[0m`;
const Y = (x) => `\x1b[33m${x}\x1b[0m`;
const R = (x) => `\x1b[31m${x}\x1b[0m`;
const C = (x) => `\x1b[36m${x}\x1b[0m`;
const statusColor = (st) => (st === 'Green' ? G(st) : st === 'Yellow' ? Y(st) : st === 'Red' ? R(st) : D(st));
const readyColor = (r) => (r === 'Ready' ? G(r) : r === 'Not ready' ? Y(r) : D(r));

let state = M.createSeedState();
let lens = 'dash'; // dash | review | hist
let histWeek = 1;
let focusTeam = 'T2'; // Sales — has the most contrasts seeded
let flash = '';

const teamName = (id) => (M.teamById(state, id) || {}).name || id;
const focus = () => M.teamById(state, focusTeam);

// ===========================================================================
// RENDER
// ===========================================================================
function header() {
  const wk = M.weekLabel(state.currentWeek);
  const lensLabel = { dash: 'DASHBOARD (input)', review: 'WEEKLY PULSE REVIEW', hist: 'MEETING HISTORY' }[lens];
  return (
    B(`OCE Weekly Pulse — ${lensLabel}`) +
    '\n' +
    D(`current ${wk}   ·   focus team: `) + C(focus().name) +
    D(`   ·   lens [1]dash [2]review [3]history`) +
    '\n' + D('─'.repeat(78))
  );
}

function renderDashboard() {
  const out = [];
  for (const row of M.dashboard(state)) {
    const lead = M.personName(state, row.team.leaderId);
    out.push(`${B(row.team.name)} ${D('· lead ' + lead)}   readiness: ${readyColor(row.readiness)}`);
    // check-in
    out.push(
      '  ' + D('check-in: ') +
        (row.checkin ? `${row.checkin.type} — ${row.checkin.note}` : D('— (blank this week)'))
    );
    // kpis
    const kpiBits = row.kpis.map((k) => {
      const scope = k.def.inScope ? '' : D('[tracked-only]');
      const active = k.def.active ? '' : D('[retired]');
      const res = k.result ? statusColor(k.result.status) + ` ${k.result.actual}` : D('no result');
      return `${k.def.id} ${k.def.name} ${D('(' + k.def.target + ')')} ${scope}${active} → ${res}`;
    });
    if (kpiBits.length) out.push('  ' + D('KPIs: ') + kpiBits.join(D('  |  ')));
    // headlines
    for (const h of row.headlines)
      out.push('  ' + D('headline: ') + (h.type === 'Risk' ? R('Risk') : 'Highlight') + ` ${h.id} — ${h.text}` + (h.pin ? C(' [pinned]') : ''));
    // issues (open + deferred)
    for (const i of row.issues) {
      const tag = i.state === 'Deferred' ? D('[Deferred — quiet]') : '';
      out.push('  ' + D('issue: ') + `${i.entity.id} ${i.entity.title} ${D('(' + i.entity.priority + ')')} ${stateTag(i.state)} ${tag}` + pinTag(i.entity) + originTag(i.entity));
    }
    // todos
    for (const d of row.todos) {
      const tag = d.state === 'Blocked' ? R('[Blocked — loud]') : '';
      out.push('  ' + D('to-do: ') + `${d.entity.id} ${d.entity.title} ${stateTag(d.state)} ${tag}` + originTag(d.entity));
    }
    out.push('');
  }
  return out.join('\n');
}

function renderReview() {
  const out = [D('Surfaced = what the meeting actually walks through. Quiet items are hidden.'), ''];
  for (const row of M.reviewSurfacing(state, state.currentWeek)) {
    const loudKpis = row.kpis.filter((k) => k.loud);
    const quietKpis = row.kpis.filter((k) => !k.loud);
    if (!loudKpis.length && !quietKpis.length && !row.issues.length && !row.todos.length) continue;
    out.push(B(row.team.name) + `   ${D('readiness:')} ${readyColor(M.readiness(state, row.team.id, state.currentWeek))}`);
    for (const k of loudKpis)
      out.push('  ' + statusColor(k.status) + ` ${k.def.id} ${k.def.name}` + (k.result ? ` — ${k.result.actual}` : D(' — missing')));
    if (quietKpis.length) out.push('  ' + D('quiet (Green): ' + quietKpis.map((k) => k.def.name).join(', ')));
    for (const i of row.issues) {
      const why = i.autoSurface ? D('[open]') : C('[pinned]');
      out.push('  ' + D('issue: ') + `${i.entity.id} ${i.entity.title} ${D('(' + i.entity.priority + ')')} ${why}` + originTag(i.entity));
    }
    for (const d of row.todos) {
      const tag = d.blocked ? R('[BLOCKED]') : D('[open]');
      out.push('  ' + D('to-do: ') + `${d.entity.id} ${d.entity.title} ${tag}` + originTag(d.entity));
    }
    out.push('');
  }
  out.push(D('Note who is NOT here: Deferred issues + Done to-dos + out-of-scope KPIs + Green KPIs.'));
  return out.join('\n');
}

function renderHistory() {
  const h = M.meetingHistory(state, histWeek);
  const out = [B(`Minutes — ${h.label}`) + D(`   (history nav: [<] prev  [>] next week)`), ''];
  out.push(D('Snapshots (this week only):'));
  for (const c of h.checkins) out.push(`  check-in ${teamName(c.teamId)}: ${c.type} — ${c.note}`);
  for (const r of h.kpiResults) {
    const k = state.kpiDefs.find((x) => x.id === r.kpiId);
    out.push(`  KPI ${k ? k.name : r.kpiId}: ${statusColor(r.status)} ${r.actual}`);
  }
  for (const hl of h.headlines) out.push(`  headline ${teamName(hl.teamId)}: ${hl.type} — ${hl.text}`);
  if (!h.checkins.length && !h.kpiResults.length && !h.headlines.length) out.push(D('  (none recorded)'));
  out.push('');
  out.push(D('Living entities active this week (shown as of this week):'));
  if (!h.living.length) out.push(D('  (none active)'));
  for (const l of h.living) {
    out.push(`  ${l.entity.id} ${l.entity.title} ${D('· ' + teamName(l.entity.teamId))} → ${stateTag(l.state)}` + originTag(l.entity));
    for (const ev of l.thisWeekEvents) out.push('      ' + D('· ' + eventLabel(ev)));
    if (!l.thisWeekEvents.length) out.push('      ' + D('· (surfaced, no change this week)'));
  }
  return out.join('\n');
}

function eventLabel(ev) {
  if (ev.type === 'created') return `created (${ev.detail})`;
  if (ev.type === 'status') return `status → ${ev.detail}`;
  return `comment: ${ev.detail}`;
}
function stateTag(st) {
  if (st === 'Solved' || st === 'Done') return G(`[${st}]`);
  if (st === 'Deferred') return D(`[${st}]`);
  if (st === 'Blocked') return R(`[${st}]`);
  return `[${st}]`;
}
function pinTag(e) {
  return e.pin ? C(' [pinned]') : '';
}
function originTag(e) {
  return e.origin ? D(` ←${e.origin.type} ${e.origin.id}`) : '';
}

const FOOTER = [
  D('VIEWS  ') + '1 dash  2 review  3 history  ' + B('w') + D(' next week') + '  ' + B('<')+ D('/') + B('>') + D(' hist week') + '  ' + B('t <Tid>') + D(' focus team'),
  D('SNAP   ') + 'ci <Win|Concern|Priority|Other> <note>   res <Kid> <G|Y|R> <actual>   hl <Highlight|Risk> <text>',
  D('LIVING ') + 'issue <title>   todo <title>   st <Iid|Did> <state>   cm <id> <text>   pin <id>   pri <Iid> <Low|Med|High|Critical>',
  D('CONV   ') + 'cv.ci <Cid>   cv.hl <Hid>   cv.td <Iid> [meeting]      OTHER  scope <Kid>  active <Kid>  summary  show <id>  seed  q',
].join('\n');

function render() {
  process.stdout.write('\x1b[2J\x1b[H');
  let body = lens === 'dash' ? renderDashboard() : lens === 'review' ? renderReview() : renderHistory();
  console.log(header());
  console.log(body);
  console.log(D('─'.repeat(78)));
  if (flash) {
    console.log(C('» ' + flash));
    flash = '';
  }
  console.log(FOOTER);
}

// ===========================================================================
// COMMAND DISPATCH
// ===========================================================================
function dispatch(line) {
  const parts = line.trim().split(/\s+/);
  const cmd = parts[0];
  const rest = line.trim().slice(cmd.length).trim();
  const arg = (n) => parts[n];
  const statusMap = { g: 'Green', y: 'Yellow', r: 'Red', green: 'Green', yellow: 'Yellow', red: 'Red' };
  const issueStateMap = { open: 'Open', deferred: 'Deferred', solved: 'Solved' };
  const todoStateMap = { open: 'Open', blocked: 'Blocked', done: 'Done' };

  try {
    switch (cmd) {
      case '': return;
      case '1': lens = 'dash'; break;
      case '2': lens = 'review'; break;
      case '3': lens = 'hist'; histWeek = state.currentWeek; break;
      case 'w':
        M.advanceWeek(state);
        lens = 'dash';
        flash = `Advanced to ${M.weekLabel(state.currentWeek)} — snapshots blank, living items carried over.`;
        break;
      case '<': if (histWeek > 1) histWeek--; lens = 'hist'; break;
      case '>': if (histWeek < state.currentWeek) histWeek++; lens = 'hist'; break;
      case 't': {
        const t = M.teamById(state, arg(1)) || state.teams.find((x) => x.name.toLowerCase() === rest.toLowerCase());
        if (t) { focusTeam = t.id; flash = `Focus → ${t.name}`; } else flash = 'no such team';
        break;
      }
      case 'ci': {
        const type = capitalize(arg(1));
        M.upsertCheckin(state, focusTeam, type, after(rest, arg(1)));
        flash = `check-in upserted for ${focus().name}`;
        break;
      }
      case 'res': {
        M.setKpiResult(state, arg(1), statusMap[arg(2).toLowerCase()], after2(rest, arg(1), arg(2)));
        flash = `KPI result set for ${arg(1)}`;
        break;
      }
      case 'hl':
        M.addHeadline(state, focusTeam, capitalize(arg(1)), after(rest, arg(1)));
        flash = `headline added to ${focus().name}`;
        break;
      case 'issue': {
        const i = M.addIssue(state, focusTeam, rest);
        flash = `issue ${i.id} created in ${focus().name}`;
        break;
      }
      case 'todo': {
        const d = M.addTodo(state, focusTeam, rest);
        flash = `to-do ${d.id} created in ${focus().name}`;
        break;
      }
      case 'st': {
        const e = M.livingById(state, arg(1));
        if (!e) { flash = 'no such item'; break; }
        const map = e.kind === 'issue' ? issueStateMap : todoStateMap;
        M.setLivingState(state, arg(1), map[arg(2).toLowerCase()]);
        flash = `${arg(1)} → ${map[arg(2).toLowerCase()]}`;
        break;
      }
      case 'cm':
        M.addComment(state, arg(1), after(rest, arg(1)));
        flash = `comment added to ${arg(1)} (now active in ${M.weekLabel(state.currentWeek)})`;
        break;
      case 'pin': {
        const e = M.togglePin(state, arg(1));
        flash = `${arg(1)} pin → ${e.pin}`;
        break;
      }
      case 'pri':
        M.setPriority(state, arg(1), capitalize(arg(2)));
        flash = `${arg(1)} priority → ${capitalize(arg(2))}`;
        break;
      case 'scope': {
        const k = state.kpiDefs.find((x) => x.id === arg(1));
        if (k) { k.inScope = !k.inScope; flash = `${k.id} in-scope → ${k.inScope}`; } else flash = 'no such KPI';
        break;
      }
      case 'active': {
        const k = state.kpiDefs.find((x) => x.id === arg(1));
        if (k) { k.active = !k.active; flash = `${k.id} active → ${k.active}`; } else flash = 'no such KPI';
        break;
      }
      case 'cv.ci': { const i = M.convertCheckinToIssue(state, arg(1)); flash = `check-in ${arg(1)} → issue ${i.id} (source kept)`; break; }
      case 'cv.hl': { const i = M.convertHeadlineToIssue(state, arg(1)); flash = `headline ${arg(1)} → issue ${i.id} (source kept)`; break; }
      case 'cv.td': { const d = M.convertIssueToTodo(state, arg(1), arg(2) === 'meeting'); flash = `issue ${arg(1)} → to-do ${d.id} (source kept)`; break; }
      case 'summary': showSummary(); return;
      case 'show': showEntity(arg(1)); return;
      case 'seed': state = M.createSeedState(); lens = 'dash'; focusTeam = 'T2'; flash = 'reseeded'; break;
      case 'q': case 'quit': case 'exit': process.exit(0);
      default: flash = `unknown command: ${cmd}  (try ?)`;
    }
  } catch (err) {
    flash = R('error: ' + err.message);
  }
  render();
  prompt();
}

function showSummary() {
  const sum = M.buildSummary(state, state.currentWeek);
  process.stdout.write('\x1b[2J\x1b[H');
  console.log(B(`Weekly Summary — ${M.weekLabel(state.currentWeek)}`) + D(sum.edited ? '  (edited)' : '  (auto-draft)'));
  console.log(D('─'.repeat(78)));
  console.log(B('Decisions') + D(' ← issues Solved this week'));
  (sum.decisions.length ? sum.decisions : ['—']).forEach((x) => console.log('  • ' + x));
  console.log(B('Actions') + D(' ← to-dos created in-meeting this week'));
  (sum.actions.length ? sum.actions : ['—']).forEach((x) => console.log('  • ' + x));
  console.log(B('Risks') + D(' ← Risk headlines + High/Critical open issues'));
  (sum.risks.length ? sum.risks : ['—']).forEach((x) => console.log('  • ' + x));
  console.log(B('Summary text'));
  console.log('  ' + sum.text);
  console.log(D('─'.repeat(78)));
  console.log(D('(facilitator would confirm/override; press Enter to return)'));
  rl.question('', () => { render(); prompt(); });
}

function showEntity(id) {
  const e = M.livingById(state, id);
  process.stdout.write('\x1b[2J\x1b[H');
  if (!e) { console.log('no such living entity ' + id); rl.question('', () => { render(); prompt(); }); return; }
  console.log(B(`${e.id} — ${e.title}`));
  console.log(D(`kind ${e.kind} · team ${teamName(e.teamId)} · owner ${M.personName(state, e.ownerId)} · origin ${M.weekLabel(e.originWeek)}`));
  if (e.kind === 'issue') console.log(D(`priority ${e.priority} · pinned ${e.pin}`));
  if (e.origin) console.log(C(`back-link → ${e.origin.type} ${e.origin.id} (source preserved)`));
  console.log(B('\nEvent log (drives state-as-of & history):'));
  for (const ev of e.events) console.log(`  ${D(M.weekLabel(ev.week))}  ${eventLabel(ev)}`);
  console.log(D('\nActive in weeks: ') + activeWeeks(e).join(', '));
  console.log(D('(press Enter to return)'));
  rl.question('', () => { render(); prompt(); });
}
function activeWeeks(e) {
  const out = [];
  for (let w = 1; w <= state.currentWeek; w++) if (M.activeInWeek(e, w)) out.push('W' + w);
  return out;
}

// helpers to grab the free-text tail after N tokens
const after = (rest, tok) => rest.slice((tok || '').length).trim();
const after2 = (rest, a, b) => rest.slice((a || '').length).trim().slice((b || '').length).trim();
const capitalize = (x) => (x ? x[0].toUpperCase() + x.slice(1).toLowerCase() : x);

// ===========================================================================
const rl = readline.createInterface({ input: process.stdin, output: process.stdout });
function prompt() { rl.question(B('› '), (line) => dispatch(line)); }

render();
prompt();
