"""
single_agent.py  –  Use Case 1
================================
One script controls one character in the game.
The Unity display follows this agent's point-of-view.

Usage:
    python single_agent.py

Requirements:
    pip install websockets
"""

import asyncio
import json
import time
import websockets


UNITY_WS_URL = "ws://localhost:8080/agent"
AGENT_ID     = "agent_01"


# ---------------------------------------------------------------------------
# Low-level helpers
# ---------------------------------------------------------------------------

def make_command(command_type: str, data: dict, agent_id: str = AGENT_ID) -> str:
    return json.dumps({
        "commandType": command_type,
        "data":        data,
        "agentId":     agent_id,
        "timestamp":   time.time(),
    })


async def send(ws, command_type: str, data: dict = {}, agent_id: str = AGENT_ID):
    msg = make_command(command_type, data, agent_id)
    await ws.send(msg)
    print(f"[{agent_id}] → {command_type} {data}")


# ---------------------------------------------------------------------------
# High-level actions
# ---------------------------------------------------------------------------

async def move(ws, x: float, z: float):
    """Move: x = strafe, z = forward (normalized)."""
    await send(ws, "MOVE", {"x": x, "z": z})


async def stop(ws):
    await send(ws, "STOP", {})


async def look(ws, pitch: float, yaw: float):
    """Look to absolute pitch/yaw in degrees."""
    await send(ws, "LOOK", {"pitch": pitch, "yaw": yaw})


async def shoot(ws, active: bool = True, duration: float = 0):
    await send(ws, "SHOOT", {"active": active, "duration": duration})


async def reload_weapon(ws):
    await send(ws, "RELOAD", {})


async def switch_weapon(ws, slot: int):
    """slot is 0-based (0 = rifle, 1 = sniper, 2 = pistol …)."""
    await send(ws, "SWITCH_WEAPON", {"weaponIndex": slot})


async def aim(ws, active: bool):
    await send(ws, "AIM", {"active": active})


async def set_view(ws, target_agent_id: str = AGENT_ID):
    """Ask Unity to display this agent's camera."""
    await send(ws, "SET_VIEW", {"viewTargetAgentId": target_agent_id})


# ---------------------------------------------------------------------------
# Game-state listener
# ---------------------------------------------------------------------------

async def listen_state(ws):
    """Background task: print game state updates from Unity."""
    async for raw in ws:
        try:
            msg = json.loads(raw)
            if msg.get("type") == "welcome":
                print(f"[{AGENT_ID}] Connected – session {msg.get('sessionId')}")
                continue
            if "player" in msg:
                p = msg["player"]
                print(
                    f"[{AGENT_ID}] HP:{p.get('health',0):.0f}/{p.get('maxHealth',0):.0f} "
                    f"Ammo:{p.get('currentAmmo',0)}/{p.get('maxAmmo',0)} "
                    f"State:{p.get('movementState','?')} "
                    f"Enemies:{len(msg.get('enemies', []))}"
                )
        except Exception as e:
            print(f"[{AGENT_ID}] Parse error: {e}")


# ---------------------------------------------------------------------------
# Main scenario
# ---------------------------------------------------------------------------

async def main():
    print(f"[{AGENT_ID}] Connecting to {UNITY_WS_URL} ...")
    async with websockets.connect(UNITY_WS_URL) as ws:
        # Start listening to state updates in the background
        listener = asyncio.create_task(listen_state(ws))

        # --- Tell Unity to follow this agent's camera ---
        await asyncio.sleep(0.5)
        await set_view(ws)

        # --- Demonstrate all 6 controls ---

        print("\n--- Move forward ---")
        await move(ws, 0.0, 1.0)
        await asyncio.sleep(2.0)

        print("\n--- Strafe right ---")
        await move(ws, 1.0, 0.0)
        await asyncio.sleep(1.5)

        print("\n--- Sprint forward (full forward input) ---")
        await move(ws, 0.0, 1.0)
        await asyncio.sleep(2.0)

        print("\n--- Stop ---")
        await stop(ws)
        await asyncio.sleep(0.5)

        print("\n--- Look right 90° ---")
        await look(ws, 0.0, 90.0)
        await asyncio.sleep(1.0)

        print("\n--- Aim down sights ---")
        await aim(ws, True)
        await asyncio.sleep(1.0)

        print("\n--- Shoot (burst) ---")
        await shoot(ws, active=True, duration=0.5)
        await asyncio.sleep(1.0)

        print("\n--- Reload ---")
        await reload_weapon(ws)
        await asyncio.sleep(2.5)   # wait for reload animation

        print("\n--- Switch to slot 1 (sniper) ---")
        await switch_weapon(ws, 1)
        await asyncio.sleep(0.5)

        print("\n--- Switch back to slot 0 (rifle) ---")
        await switch_weapon(ws, 0)
        await asyncio.sleep(0.5)

        print("\n--- Exit ADS ---")
        await aim(ws, False)

        print("\n--- Done. Stopping. ---")
        await stop(ws)

        listener.cancel()


if __name__ == "__main__":
    asyncio.run(main())
