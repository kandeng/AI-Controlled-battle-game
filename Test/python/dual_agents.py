"""
dual_agent.py  –  Use Case 2
================================
Two agents each control one game character simultaneously.
They can be teammates or enemies.

HOW CAMERA / VIEW WORKS
------------------------
- The first agent to connect (agent_01) calls SET_VIEW to claim the display.
- The second agent (agent_02) also calls SET_VIEW, but its attempt is ignored
  because the first-connected agent already owns the view.
- You can override with:
      await set_view(ws2, target_agent_id="agent_02")
  ... which hands control of the view to agent_02's character.
- In the Unity Editor/build, a dropdown in the top-right corner lets you
  manually switch the view at any time.

TYPICAL WORKFLOW
----------------
Terminal 1:  python dual_agent.py --agent agent_01
Terminal 2:  python dual_agent.py --agent agent_02

Or run both agents in the same process (this file).

Requirements:
    pip install websockets
"""

import asyncio
import argparse
import json
import time
import websockets


UNITY_WS_URL = "ws://localhost:8080/agent"


# ---------------------------------------------------------------------------
# Wall-collision / stuck detector (one instance per agent)
# ---------------------------------------------------------------------------

class StuckDetector:
    STUCK_FRAMES = 3
    POS_EPSILON  = 0.02   # metres

    def __init__(self):
        self._last_pos    = None
        self._stuck_count = 0
        self.is_moving    = False

    def on_position(self, pos: dict) -> bool:
        if not self.is_moving:
            self._stuck_count = 0
            self._last_pos = None
            return False
        cur = (pos.get("x", 0.0), pos.get("y", 0.0), pos.get("z", 0.0))
        if self._last_pos is not None:
            if all(abs(cur[i] - self._last_pos[i]) < self.POS_EPSILON for i in range(3)):
                self._stuck_count += 1
            else:
                self._stuck_count = 0
        self._last_pos = cur
        return self._stuck_count >= self.STUCK_FRAMES


_detectors: dict[str, StuckDetector] = {}


def get_detector(agent_id: str) -> StuckDetector:
    if agent_id not in _detectors:
        _detectors[agent_id] = StuckDetector()
    return _detectors[agent_id]


# ---------------------------------------------------------------------------
# Low-level helpers (same as single_agent.py)
# ---------------------------------------------------------------------------

def make_command(command_type: str, data: dict, agent_id: str) -> str:
    return json.dumps({
        "commandType": command_type,
        "data":        data,
        "agentId":     agent_id,
        "timestamp":   time.time(),
    })


async def send(ws, command_type: str, data: dict, agent_id: str):
    msg = make_command(command_type, data, agent_id)
    await ws.send(msg)
    print(f"  [{agent_id}] → {command_type} {data}")


async def move(ws, agent_id, x, z):
    d = get_detector(agent_id); d.is_moving = True; d._stuck_count = 0
    await send(ws, "MOVE", {"x": x, "z": z}, agent_id)
async def stop(ws, agent_id):
    get_detector(agent_id).is_moving = False
    await send(ws, "STOP", {}, agent_id)
async def look(ws, agent_id, p, y):      await send(ws, "LOOK",         {"pitch": p, "yaw": y},           agent_id)
async def shoot(ws, agent_id, a, d=0):   await send(ws, "SHOOT",        {"active": a, "duration": d},     agent_id)
async def reload_w(ws, agent_id):        await send(ws, "RELOAD",       {},                               agent_id)
async def switch_w(ws, agent_id, slot):  await send(ws, "SWITCH_WEAPON",{"weaponIndex": slot},            agent_id)
async def aim(ws, agent_id, a):          await send(ws, "AIM",          {"active": a},                    agent_id)
async def set_view(ws, source_id, target_id=None):
    t = target_id if target_id else source_id
    await send(ws, "SET_VIEW", {"viewTargetAgentId": t}, source_id)


# ---------------------------------------------------------------------------
# State listener
# ---------------------------------------------------------------------------

async def listen_state(ws, agent_id: str):
    async for raw in ws:
        try:
            msg = json.loads(raw)
            if msg.get("type") == "welcome":
                print(f"[{agent_id}] Connected – session {msg.get('sessionId')}")
                continue
            if "player" in msg:
                p = msg["player"]
                print(
                    f"  [{agent_id}] HP:{p.get('health',0):.0f} "
                    f"Ammo:{p.get('currentAmmo',0)} "
                    f"State:{p.get('movementState','?')}"
                )
                d = get_detector(agent_id)
                if "position" in p and d.on_position(p["position"]):
                    print(f"  [{agent_id}] Wall detected — auto-stop.")
                    d.is_moving = False
                    await send(ws, "STOP", {}, agent_id)
        except Exception:
            pass


# ---------------------------------------------------------------------------
# Per-agent scenario
# ---------------------------------------------------------------------------

async def run_agent(agent_id: str, claim_view: bool):
    """
    Connect to Unity, optionally claim the camera, then run a short scenario.
    claim_view=True  → this agent's character is shown on the Unity display.
    """
    print(f"[{agent_id}] Connecting to {UNITY_WS_URL} ...")
    async with websockets.connect(UNITY_WS_URL) as ws:
        listener = asyncio.create_task(listen_state(ws, agent_id))

        await asyncio.sleep(0.3)

        # --- View ownership ---
        if claim_view:
            print(f"\n[{agent_id}] Claiming camera view...")
            await set_view(ws, agent_id)

        # --- Scenario ---
        await asyncio.sleep(0.5)
        print(f"\n[{agent_id}] Moving forward...")
        await move(ws, agent_id, 0.0, 1.0)
        await asyncio.sleep(3.0)

        print(f"\n[{agent_id}] Looking left 45°...")
        await look(ws, agent_id, 0.0, -45.0)
        await asyncio.sleep(1.0)

        print(f"\n[{agent_id}] Shooting burst...")
        await shoot(ws, agent_id, True, 0.4)
        await asyncio.sleep(0.8)

        print(f"\n[{agent_id}] Reloading...")
        await reload_w(ws, agent_id)
        await asyncio.sleep(2.5)

        print(f"\n[{agent_id}] Stop.")
        await stop(ws, agent_id)

        # --- Optionally hand view to other agent ---
        if claim_view:
            other = "agent_02" if agent_id == "agent_01" else "agent_01"
            print(f"\n[{agent_id}] Handing view to {other}...")
            await set_view(ws, agent_id, target_id=other)
            await asyncio.sleep(3.0)

        listener.cancel()
        print(f"[{agent_id}] Done.")


# ---------------------------------------------------------------------------
# Entry points
# ---------------------------------------------------------------------------

async def run_both_in_one_process():
    """Run agent_01 and agent_02 concurrently in the same script."""
    print("=== Dual Agent Demo (both in one process) ===\n")
    await asyncio.gather(
        run_agent("agent_01", claim_view=True),
        run_agent("agent_02", claim_view=False),
    )


async def run_single(agent_id: str):
    """Run one agent (meant to be launched from a separate terminal)."""
    claim = (agent_id == "agent_01")
    await run_agent(agent_id, claim_view=claim)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Dual-agent WebSocket client")
    parser.add_argument(
        "--agent",
        default=None,
        help="Agent ID to run ('agent_01' or 'agent_02'). "
             "Omit to run both concurrently in one process.",
    )
    args = parser.parse_args()

    if args.agent:
        asyncio.run(run_single(args.agent))
    else:
        asyncio.run(run_both_in_one_process())
