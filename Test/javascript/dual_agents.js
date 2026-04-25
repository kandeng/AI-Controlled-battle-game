/**
 * dual_agents.js  –  Use Case 2
 * ================================
 * Two agents each control one game character simultaneously.
 * They can be teammates or enemies.
 *
 * HOW CAMERA / VIEW WORKS
 * ------------------------
 * - The first agent to connect (agent_01) calls SET_VIEW to claim the display.
 * - agent_02 can take over by calling SET_VIEW with its own agentId.
 * - Either agent can point the view at the other with viewTargetAgentId.
 * - In Unity Editor / Development builds a dropdown in the top-right corner
 *   lets you manually switch the view at any time.
 *
 * TYPICAL WORKFLOW
 * ----------------
 * Terminal 1:  node dual_agents.js --agent agent_01
 * Terminal 2:  node dual_agents.js --agent agent_02
 *
 * Or run both agents in the same process (default):
 *   node dual_agents.js
 *
 * Requirements:
 *   npm install ws        (already in package.json)
 */

'use strict';

const WebSocket = require('ws');

const UNITY_WS_URL = 'ws://localhost:8080/agent';

// ---------------------------------------------------------------------------
// Low-level helpers
// ---------------------------------------------------------------------------

function makeCommand(commandType, data = {}, agentId) {
  return JSON.stringify({
    commandType,
    data,
    agentId,
    timestamp: Date.now() / 1000,
  });
}

function send(ws, commandType, data = {}, agentId) {
  ws.send(makeCommand(commandType, data, agentId));
  console.log(`  [${agentId}] → ${commandType}`, data);
}

function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

// ---------------------------------------------------------------------------
// High-level actions (all require explicit agentId)
// ---------------------------------------------------------------------------

const move         = (ws, id, x, z)       => send(ws, 'MOVE',         { x, z },               id);
const stop         = (ws, id)             => send(ws, 'STOP',         {},                      id);
const look         = (ws, id, pitch, yaw) => send(ws, 'LOOK',         { pitch, yaw },          id);
const shoot        = (ws, id, active, d=0)=> send(ws, 'SHOOT',        { active, duration: d }, id);
const reloadWeapon = (ws, id)             => send(ws, 'RELOAD',       {},                      id);
const switchWeapon = (ws, id, slot)       => send(ws, 'SWITCH_WEAPON',{ weaponIndex: slot },   id);
const aim          = (ws, id, active)     => send(ws, 'AIM',          { active },              id);
const setView      = (ws, sourceId, targetId) =>
  send(ws, 'SET_VIEW', { viewTargetAgentId: targetId || sourceId }, sourceId);

// ---------------------------------------------------------------------------
// Game-state listener
// ---------------------------------------------------------------------------

function makeListener(agentId) {
  return function onMessage(raw) {
    try {
      const msg = JSON.parse(raw);
      if (msg.type === 'welcome') {
        console.log(`[${agentId}] Connected – session ${msg.sessionId}`);
        return;
      }
      if (msg.player) {
        const p = msg.player;
        console.log(
          `  [${agentId}] HP:${p.health} ` +
          `Ammo:${p.currentAmmo} ` +
          `State:${p.movementState}`
        );
      }
    } catch (_) { /* ignore */ }
  };
}

// ---------------------------------------------------------------------------
// Per-agent scenario
// ---------------------------------------------------------------------------

async function connectAgent(agentId) {
  console.log(`[${agentId}] Connecting to ${UNITY_WS_URL} ...`);
  const ws = new WebSocket(UNITY_WS_URL);
  await new Promise((resolve, reject) => {
    ws.once('open',  resolve);
    ws.once('error', reject);
  });
  ws.on('message', makeListener(agentId));
  ws.on('close', (code) => console.log(`[${agentId}] Closed (${code})`));
  return ws;
}

async function runAgent(agentId, claimView) {
  const ws = await connectAgent(agentId);
  const other = agentId === 'agent_01' ? 'agent_02' : 'agent_01';

  await sleep(300);

  // --- View ownership ---
  if (claimView) {
    console.log(`\n[${agentId}] Claiming camera view...`);
    setView(ws, agentId);
  }

  await sleep(500);

  console.log(`\n[${agentId}] Moving forward...`);
  move(ws, agentId, 0, 1);
  await sleep(3000);

  console.log(`\n[${agentId}] Looking left 45°...`);
  look(ws, agentId, 0, -45);
  await sleep(1000);

  console.log(`\n[${agentId}] Shooting burst...`);
  shoot(ws, agentId, true, 0.4);
  await sleep(800);

  console.log(`\n[${agentId}] Reloading...`);
  reloadWeapon(ws, agentId);
  await sleep(2500);

  console.log(`\n[${agentId}] Stop.`);
  stop(ws, agentId);

  // --- Optionally hand view to the other agent ---
  if (claimView) {
    console.log(`\n[${agentId}] Handing view to ${other}...`);
    setView(ws, agentId, other);
    await sleep(3000);
  }

  ws.close();
  console.log(`[${agentId}] Done.`);
}

// ---------------------------------------------------------------------------
// Entry points
// ---------------------------------------------------------------------------

async function runBothInOneProcess() {
  console.log('=== Dual Agent Demo (both in one process) ===\n');
  await Promise.all([
    runAgent('agent_01', /* claimView */ true),
    runAgent('agent_02', /* claimView */ false),
  ]);
}

async function runSingle(agentId) {
  await runAgent(agentId, agentId === 'agent_01');
}

// Parse optional --agent flag
const args    = process.argv.slice(2);
const agentIdx = args.indexOf('--agent');
const agentArg = agentIdx !== -1 ? args[agentIdx + 1] : null;

if (agentArg) {
  runSingle(agentArg).catch(err => {
    console.error('Fatal error:', err.message);
    process.exit(1);
  });
} else {
  runBothInOneProcess().catch(err => {
    console.error('Fatal error:', err.message);
    process.exit(1);
  });
}
