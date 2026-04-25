/**
 * single_agent.js  –  Use Case 1
 * ================================
 * One script controls one character in the game.
 * The Unity display follows this agent's point-of-view.
 *
 * Usage:
 *   node single_agent.js
 *
 * Requirements:
 *   npm install ws        (already in package.json)
 */

'use strict';

const WebSocket = require('ws');

const UNITY_WS_URL = 'ws://localhost:8080/agent';
const AGENT_ID     = 'agent_01';

// ---------------------------------------------------------------------------
// Low-level helpers
// ---------------------------------------------------------------------------

function makeCommand(commandType, data = {}, agentId = AGENT_ID) {
  return JSON.stringify({
    commandType,
    data,
    agentId,
    timestamp: Date.now() / 1000,
  });
}

function send(ws, commandType, data = {}, agentId = AGENT_ID) {
  ws.send(makeCommand(commandType, data, agentId));
  console.log(`[${agentId}] → ${commandType}`, data);
}

function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

// ---------------------------------------------------------------------------
// Wall-collision / stuck detector
// ---------------------------------------------------------------------------

const STUCK_FRAMES = 3;
const POS_EPSILON  = 0.02;  // metres

const stuck = {
  isMoving:   false,
  lastPos:    null,
  stuckCount: 0,

  /** Call with msg.player.position each time a broadcast arrives. */
  onPosition(pos) {
    if (!this.isMoving) {
      this.stuckCount = 0;
      this.lastPos    = null;
      return false;
    }
    if (this.lastPos) {
      const same =
        Math.abs(pos.x - this.lastPos.x) < POS_EPSILON &&
        Math.abs(pos.y - this.lastPos.y) < POS_EPSILON &&
        Math.abs(pos.z - this.lastPos.z) < POS_EPSILON;
      this.stuckCount = same ? this.stuckCount + 1 : 0;
    }
    this.lastPos = { ...pos };
    return this.stuckCount >= STUCK_FRAMES;
  },
};

// ---------------------------------------------------------------------------
// High-level actions
// ---------------------------------------------------------------------------

const move = (ws, x, z) => {
  stuck.isMoving = true; stuck.stuckCount = 0;
  send(ws, 'MOVE', { x, z });
};
const stop = (ws) => {
  stuck.isMoving = false;
  send(ws, 'STOP', {});
};
const look         = (ws, pitch, yaw)=> send(ws, 'LOOK',         { pitch, yaw });
const shoot        = (ws, active, duration = 0) => send(ws, 'SHOOT', { active, duration });
const reloadWeapon = (ws)            => send(ws, 'RELOAD',       {});
const switchWeapon = (ws, slot)      => send(ws, 'SWITCH_WEAPON',{ weaponIndex: slot });
const aim          = (ws, active)    => send(ws, 'AIM',          { active });
const setView      = (ws, targetId = AGENT_ID) =>
  send(ws, 'SET_VIEW', { viewTargetAgentId: targetId });

// ---------------------------------------------------------------------------
// Game-state listener
// ---------------------------------------------------------------------------

function onMessage(raw, ws) {
  try {
    const msg = JSON.parse(raw);
    if (msg.type === 'welcome') {
      console.log(`[${AGENT_ID}] Connected – session ${msg.sessionId}`);
      return;
    }
    if (msg.player) {
      const p = msg.player;
      console.log(
        `[${AGENT_ID}] HP:${p.health}/${p.maxHealth} ` +
        `Ammo:${p.currentAmmo}/${p.maxAmmo} ` +
        `State:${p.movementState} ` +
        `Enemies:${(msg.enemies || []).length}`
      );
      if (p.position && stuck.onPosition(p.position)) {
        console.log(`[${AGENT_ID}] Wall detected — auto-stop.`);
        stuck.isMoving = false;
        send(ws, 'STOP', {});
      }
    }
  } catch (e) {
    console.error(`[${AGENT_ID}] Parse error:`, e.message);
  }
}

// ---------------------------------------------------------------------------
// Main scenario
// ---------------------------------------------------------------------------

async function main() {
  console.log(`[${AGENT_ID}] Connecting to ${UNITY_WS_URL} ...`);

  const ws = new WebSocket(UNITY_WS_URL);

  await new Promise((resolve, reject) => {
    ws.once('open',  resolve);
    ws.once('error', reject);
  });

  ws.on('message', raw => onMessage(raw, ws));
  ws.on('close',   (code, reason) =>
    console.log(`[${AGENT_ID}] Connection closed: ${code} – ${reason}`));

  // --- Tell Unity to follow this agent's camera ---
  await sleep(400);
  setView(ws);

  // --- Demonstrate all 6 controls ---

  console.log('\n--- Move forward ---');
  move(ws, 0, 1);
  await sleep(2000);

  console.log('\n--- Strafe right ---');
  move(ws, 1, 0);
  await sleep(1500);

  console.log('\n--- Sprint forward ---');
  move(ws, 0, 1);
  await sleep(2000);

  console.log('\n--- Stop ---');
  stop(ws);
  await sleep(500);

  console.log('\n--- Look right 90° ---');
  look(ws, 0, 90);
  await sleep(1000);

  console.log('\n--- Aim down sights ---');
  aim(ws, true);
  await sleep(1000);

  console.log('\n--- Shoot (0.5 s burst) ---');
  shoot(ws, true, 0.5);
  await sleep(1000);

  console.log('\n--- Reload ---');
  reloadWeapon(ws);
  await sleep(2500);

  console.log('\n--- Switch to slot 1 (sniper) ---');
  switchWeapon(ws, 1);
  await sleep(500);

  console.log('\n--- Switch back to slot 0 (rifle) ---');
  switchWeapon(ws, 0);
  await sleep(500);

  console.log('\n--- Exit ADS ---');
  aim(ws, false);

  console.log('\n--- Done. Stopping. ---');
  stop(ws);

  await sleep(500);
  ws.close();
}

main().catch(err => {
  console.error('Fatal error:', err.message);
  process.exit(1);
});
