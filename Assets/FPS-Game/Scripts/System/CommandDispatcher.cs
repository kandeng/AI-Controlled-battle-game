using System.Collections;
using UnityEngine;

/// <summary>
/// Single shared command router. Receives AgentCommand objects from
/// WebSocketServerManager or DebugConsole, validates them, and forwards
/// to IPlayerCommandAPI (PlayerCommandAPI).
///
/// Attach to the GameSystems or InGameManager GameObject alongside
/// WebSocketServerManager and CoroutineManager.
/// </summary>
public class CommandDispatcher : MonoBehaviour
{
    public static CommandDispatcher Instance { get; private set; }

    [SerializeField] PlayerCommandAPI _api;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_api == null)
            _api = GetComponent<PlayerCommandAPI>();

        // Auto-create the view selector overlay if not already present
        if (ViewSelectorUI.Instance == null)
        {
            var go = new GameObject("ViewSelectorUI");
            go.AddComponent<ViewSelectorUI>();
        }
    }

    // -------------------------------------------------------
    // Public entry point
    // -------------------------------------------------------

    /// <summary>
    /// Validate and dispatch a command from any source (WebSocket or DebugConsole).
    /// </summary>
    public void Dispatch(AgentCommand command)
    {
        if (!ValidateCommand(command))
        {
            Debug.LogWarning($"[CommandDispatcher] Rejected command: {command?.commandType}");
            return;
        }

        if (_api == null)
        {
            _api = PlayerCommandAPI.Instance;
            if (_api == null)
            {
                Debug.LogWarning("[CommandDispatcher] No PlayerCommandAPI found in scene.");
                return;
            }
        }

        switch (command.commandType.ToUpper())
        {
            case "MOVE":         DispatchMove(command);         break;
            case "LOOK":         DispatchLook(command);         break;
            case "SHOOT":        DispatchShoot(command);        break;
            case "RELOAD":       DispatchReload(command);       break;
            case "SWITCH_WEAPON": DispatchSwitchWeapon(command); break;
            case "AIM":          DispatchAim(command);          break;
            case "STOP":         DispatchStop(command);         break;
            case "SET_VIEW":     DispatchSetView(command);      break;
            default:
                Debug.LogWarning($"[CommandDispatcher] Unknown command type: {command.commandType}");
                break;
        }
    }

    // -------------------------------------------------------
    // Validation
    // -------------------------------------------------------

    bool ValidateCommand(AgentCommand command)
    {
        if (command == null)
        {
            Debug.LogWarning("[CommandDispatcher] Null command");
            return false;
        }

        if (string.IsNullOrEmpty(command.commandType))
        {
            Debug.LogWarning("[CommandDispatcher] Empty commandType");
            return false;
        }

        // Reject commands older than 5 seconds.
        // Clients may send Unix epoch (timestamp > 1_000_000) or Unity Time.time.
        double age;
        if (command.timestamp > 1_000_000.0)
        {
            // Unix epoch seconds — compare against wall-clock UTC
            double nowEpoch = (System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
            age = nowEpoch - command.timestamp;
        }
        else
        {
            // Unity Time.time
            age = Time.time - command.timestamp;
        }
        if (age > 5.0)
        {
            Debug.LogWarning($"[CommandDispatcher] Command too old ({age:F2}s): {command.commandType}");
            return false;
        }

        // Per-type data validation
        string type = command.commandType.ToUpper();
        if (type == "MOVE" && command.data != null)
        {
            if (command.data.direction.magnitude > 1.5f)
            {
                Debug.LogWarning("[CommandDispatcher] MOVE direction magnitude out of range");
                return false;
            }
        }

        if (type == "LOOK" && command.data != null)
        {
            if (Mathf.Abs(command.data.pitch) > 90f || Mathf.Abs(command.data.yaw) > 180f)
            {
                Debug.LogWarning("[CommandDispatcher] LOOK angles out of range");
                return false;
            }
        }

        return true;
    }

    // -------------------------------------------------------
    // Dispatch helpers
    // -------------------------------------------------------

    void DispatchMove(AgentCommand cmd)
    {
        Vector3 dir = cmd.data?.direction ?? Vector3.zero;
        _api.MoveFor(cmd.agentId, new Vector2(dir.x, dir.z));
    }

    void DispatchLook(AgentCommand cmd)
    {
        float pitch = cmd.data?.pitch ?? 0f;
        float yaw   = cmd.data?.yaw   ?? 0f;
        _api.LookFor(cmd.agentId, pitch, yaw);
    }

    void DispatchShoot(AgentCommand cmd)
    {
        bool active = cmd.data?.active ?? true;
        _api.ShootFor(cmd.agentId, active);
        float duration = cmd.data?.duration ?? 0f;
        if (duration > 0f)
            StartCoroutine(StopShootAfterDelay(cmd.agentId, duration));
    }

    void DispatchReload(AgentCommand cmd)
    {
        _api.ReloadFor(cmd.agentId);
    }

    void DispatchSwitchWeapon(AgentCommand cmd)
    {
        int slot = cmd.data?.weaponIndex ?? 0;
        _api.SwitchWeaponFor(cmd.agentId, slot);
    }

    void DispatchAim(AgentCommand cmd)
    {
        bool active = cmd.data?.active ?? false;
        _api.AimFor(cmd.agentId, active);
    }

    void DispatchStop(AgentCommand cmd)
    {
        _api.MoveFor(cmd.agentId, Vector2.zero);
        _api.ShootFor(cmd.agentId, false);
    }

    void DispatchSetView(AgentCommand cmd)
    {
        // data.agentId field is not used — the target is identified by cmd.agentId itself.
        // Sending SET_VIEW with your own agentId points the camera at your character.
        // Sending SET_VIEW with another agent's agentId switches to their view.
        string targetId = cmd.data != null && !string.IsNullOrEmpty(cmd.data.viewTargetAgentId)
            ? cmd.data.viewTargetAgentId
            : cmd.agentId;
        _api.SetView(cmd.agentId, targetId);
    }

    // -------------------------------------------------------
    // Coroutine helpers
    // -------------------------------------------------------

    IEnumerator StopShootAfterDelay(string agentId, float delay)
    {
        yield return new WaitForSeconds(delay);
        _api.ShootFor(agentId, false);
    }
}
