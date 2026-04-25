/**
 * console.js  –  Use Case 3 (JavaScript)
 * =========================================
 * Interactive REPL that sends WebSocket commands to the running Unity game.
 * Type commands at the prompt; the character responds immediately.
 *
 * Usage:
 *   cd Test/console
 *   node console.js
 *
 * Requirements:
 *   npm install ws readline   (ws is already in Test/javascript/package.json)
 *   Or install locally: npm init -y && npm install ws
 */

'use strict';

const WebSocket = require('ws');
const readline  = require('readline');

const UNITY_WS_URL = 'ws://localhost:8080/agent';
const AGENT_ID     = 'console';

const HELP = `
Available commands:
  fwd / back / left / right   Move in that direction
  stop                        Stop movement
  look <pitch> <yaw>          Set camera angle (degrees)
  shoot                       Fire a short burst
  reload                      Reload current weapon
  sw <slot>                   Switch to weapon slot (0, 1, 2 ...)
  aim on / aim off            Enter / exit ADS
  view                        Claim display for this console
  help                        Show this message
  quit / exit                 Disconnect and exit
`;

// ---------------------------------------------------------------------------
// Wall-collision / stuck detector
// ---------------------------------------------------------------------------

const STUCK_FRAMES = 3;
const POS_EPSILON  = 0.02;  // metres

const stuck = {
  isMoving:   false,
  lastPos:    null,
  stuckCount: 0,
  onPosition(pos) {
    if (!this.isMoving) { this.stuckCount = 0; this.lastPos = null; return false; }
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
// Low-level helpers
// ---------------------------------------------------------------------------

function makeCommand(commandType, data = {}) {
  return JSON.stringify({
    commandType,
    data,
    agentId:   AGENT_ID,
    timestamp: Date.now() / 1000,
  });
}

function send(ws, commandType, data = {}) {
  ws.send(makeCommand(commandType, data));
  const dataStr = Object.keys(data).length ? JSON.stringify(data) : '';
  console.log(`  → ${commandType} ${dataStr}`);
}

// ---------------------------------------------------------------------------
// Command parser
// ---------------------------------------------------------------------------

function dispatch(ws, line, rl) {
  const parts = line.trim().split(/\s+/);
  if (!parts[0]) return true;

  const cmd = parts[0].toLowerCase();

  switch (cmd) {
    case 'fwd':
    case 'forward':
      stuck.isMoving = true; stuck.stuckCount = 0;
      send(ws, 'MOVE', { x: 0, z: 1 });
      break;
    case 'back':
    case 'backward':
      stuck.isMoving = true; stuck.stuckCount = 0;
      send(ws, 'MOVE', { x: 0, z: -1 });
      break;
    case 'left':
      stuck.isMoving = true; stuck.stuckCount = 0;
      send(ws, 'MOVE', { x: -1, z: 0 });
      break;
    case 'right':
      stuck.isMoving = true; stuck.stuckCount = 0;
      send(ws, 'MOVE', { x: 1, z: 0 });
      break;
    case 'stop':
      stuck.isMoving = false;
      send(ws, 'STOP');
      break;
    case 'look':
      if (parts.length < 3) {
        console.log('  Usage: look <pitch> <yaw>');
      } else {
        send(ws, 'LOOK', { pitch: parseFloat(parts[1]), yaw: parseFloat(parts[2]) });
      }
      break;
    case 'shoot':
      send(ws, 'SHOOT', { active: true, duration: 0.4 });
      break;
    case 'reload':
      send(ws, 'RELOAD');
      break;
    case 'sw':
    case 'switch':
      if (parts.length < 2) {
        console.log('  Usage: sw <slot>');
      } else {
        send(ws, 'SWITCH_WEAPON', { weaponIndex: parseInt(parts[1], 10) });
      }
      break;
    case 'aim': {
      const active = parts.length < 2 || parts[1].toLowerCase() !== 'off';
      send(ws, 'AIM', { active });
      break;
    }
    case 'view':
      send(ws, 'SET_VIEW', { viewTargetAgentId: AGENT_ID });
      break;
    case 'help':
    case '?':
    case 'h':
      console.log(HELP);
      break;
    case 'quit':
    case 'exit':
    case 'q':
      stuck.isMoving = false;
      send(ws, 'STOP');
      rl.close();
      ws.close();
      return false;
    default:
      console.log(`  Unknown command: '${cmd}'. Type 'help' for a list.`);
  }

  return true;
}

// ---------------------------------------------------------------------------
// Main REPL loop
// ---------------------------------------------------------------------------

function main() {
  console.log(`Connecting to ${UNITY_WS_URL} as '${AGENT_ID}' ...`);

  const ws = new WebSocket(UNITY_WS_URL);

  ws.once('error', err => {
    console.error('Connection error:', err.message);
    console.error('Make sure Unity is running and WebSocket server is active (port 8080).');
    process.exit(1);
  });

  ws.once('open', () => {
    console.log("Connected. Type 'help' for commands, 'quit' to exit.\n");
    send(ws, 'SET_VIEW', { viewTargetAgentId: AGENT_ID });

    // Listen for broadcasts — auto-stop on wall hit
    ws.on('message', raw => {
      try {
        const msg = JSON.parse(raw);
        if (msg.player && msg.player.position && stuck.onPosition(msg.player.position)) {
          console.log('\n  [console] Wall detected \u2014 auto-stop.');
          stuck.isMoving = false;
          send(ws, 'STOP');
          rl.prompt();
        }
      } catch (_) {}
    });

    const rl = readline.createInterface({
      input:  process.stdin,
      output: process.stdout,
      prompt: 'cmd> ',
    });

    rl.prompt();

    rl.on('line', line => {
      const cont = dispatch(ws, line, rl);
      if (cont) rl.prompt();
    });

    rl.on('close', () => {
      ws.close();
      process.exit(0);
    });
  });

  ws.on('close', (code) => {
    console.log(`\nDisconnected (${code}).`);
    process.exit(0);
  });
}

main();
