"""
console.py  –  Use Case 3 (Python)
====================================
Interactive REPL that sends WebSocket commands to the running Unity game.
Type commands at the prompt; the character responds immediately.

Usage:
    cd Test/console
    python console.py

Requirements:
    pip install websockets
"""

import asyncio
import json
import time
import websockets


UNITY_WS_URL = "ws://localhost:8080/agent"
AGENT_ID     = "console"

HELP = """
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
"""


# ---------------------------------------------------------------------------
# Wall-collision / stuck detector
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


_stuck = StuckDetector()


# ---------------------------------------------------------------------------
# Low-level helpers
# ---------------------------------------------------------------------------

def make_command(command_type: str, data: dict) -> str:
    return json.dumps({
        "commandType": command_type,
        "data":        data,
        "agentId":     AGENT_ID,
        "timestamp":   time.time(),
    })


async def send(ws, command_type: str, data: dict = {}):
    await ws.send(make_command(command_type, data))
    print(f"  \u2192 {command_type} {data if data else ''}")


async def incoming_listener(ws):
    """Background task: receive broadcasts and auto-stop on wall hit."""
    async for raw in ws:
        try:
            msg = json.loads(raw)
            if "player" in msg:
                p = msg["player"]
                if "position" in p and _stuck.on_position(p["position"]):
                    print("\n  [console] Wall detected \u2014 auto-stop.")
                    _stuck.is_moving = False
                    await send(ws, "STOP", {})
        except Exception:
            pass


# ---------------------------------------------------------------------------
# Command parser
# ---------------------------------------------------------------------------

async def dispatch(ws, line: str):
    parts = line.strip().split()
    if not parts:
        return True

    cmd = parts[0].lower()

    if cmd in ("fwd", "forward"):
        _stuck.is_moving = True; _stuck._stuck_count = 0
        await send(ws, "MOVE", {"x": 0.0, "z": 1.0})
    elif cmd in ("back", "backward"):
        _stuck.is_moving = True; _stuck._stuck_count = 0
        await send(ws, "MOVE", {"x": 0.0, "z": -1.0})
    elif cmd == "left":
        _stuck.is_moving = True; _stuck._stuck_count = 0
        await send(ws, "MOVE", {"x": -0.4, "z": 0.0})
    elif cmd == "right":
        _stuck.is_moving = True; _stuck._stuck_count = 0
        await send(ws, "MOVE", {"x": 0.4, "z": 0.0})
    elif cmd == "stop":
        _stuck.is_moving = False
        await send(ws, "STOP", {})
    elif cmd == "look":
        if len(parts) < 3:
            print("  Usage: look <pitch> <yaw>")
        else:
            await send(ws, "LOOK", {"pitch": float(parts[1]), "yaw": float(parts[2])})
    elif cmd == "shoot":
        await send(ws, "SHOOT", {"active": True, "duration": 0.4})
    elif cmd == "reload":
        await send(ws, "RELOAD", {})
    elif cmd in ("sw", "switch"):
        if len(parts) < 2:
            print("  Usage: sw <slot>")
        else:
            await send(ws, "SWITCH_WEAPON", {"weaponIndex": int(parts[1])})
    elif cmd == "aim":
        active = len(parts) < 2 or parts[1].lower() != "off"
        await send(ws, "AIM", {"active": active})
    elif cmd == "view":
        await send(ws, "SET_VIEW", {"viewTargetAgentId": AGENT_ID})
    elif cmd in ("help", "?", "h"):
        print(HELP)
    elif cmd in ("quit", "exit", "q"):
        _stuck.is_moving = False
        await send(ws, "STOP", {})
        return False
    else:
        print(f"  Unknown command: '{cmd}'. Type 'help' for a list.")

    return True


# ---------------------------------------------------------------------------
# Main REPL loop
# ---------------------------------------------------------------------------

async def main():
    print(f"Connecting to {UNITY_WS_URL} as '{AGENT_ID}' ...")
    try:
        async with websockets.connect(UNITY_WS_URL) as ws:
            print("Connected. Type 'help' for commands, 'quit' to exit.\n")
            await send(ws, "SET_VIEW", {"viewTargetAgentId": AGENT_ID})

            loop = asyncio.get_event_loop()
            listener = asyncio.create_task(incoming_listener(ws))
            running = True
            while running:
                try:
                    line = await loop.run_in_executor(None, input, "cmd> ")
                    running = await dispatch(ws, line)
                except (EOFError, KeyboardInterrupt):
                    print("\nInterrupted.")
                    _stuck.is_moving = False
                    await send(ws, "STOP", {})
                    break
            listener.cancel()
    except Exception as e:
        print(f"Connection error: {e}")
        print("Make sure Unity is running and WebSocket server is active (port 8080).")


if __name__ == "__main__":
    asyncio.run(main())
