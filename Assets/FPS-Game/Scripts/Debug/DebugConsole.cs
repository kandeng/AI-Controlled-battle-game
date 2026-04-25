using UnityEngine;

/// <summary>
/// Debug control path — call these methods from code or the Unity Inspector
/// during development / play-mode testing. Each method constructs an AgentCommand
/// and routes it through CommandDispatcher, exactly the same pipeline as WebSocket.
///
/// Attach to any scene GameObject. In a release build it can be disabled/removed.
/// </summary>
public class DebugConsole : MonoBehaviour
{
    public static DebugConsole Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ------------------------------------------------------------------
    // 6 control APIs — mirrors IPlayerCommandAPI but as simple C# calls
    // ------------------------------------------------------------------

    public void Move(float x, float y)
    {
        Dispatch("MOVE", data =>
        {
            data.x = x;
            data.z = y;
        });
    }

    public void Look(float pitch, float yaw)
    {
        Dispatch("LOOK", data =>
        {
            data.pitch = pitch;
            data.yaw   = yaw;
        });
    }

    public void Shoot(bool pressed)
    {
        Dispatch("SHOOT", data =>
        {
            data.duration = pressed ? 0f : -1f;
        });
        if (!pressed)
            CommandDispatcher.Instance?.Dispatch(BuildCommand("STOP", null));
    }

    public void Reload()
    {
        Dispatch("RELOAD", null);
    }

    public void SwitchWeapon(int slot)
    {
        Dispatch("SWITCH_WEAPON", data =>
        {
            data.weaponIndex = slot;
        });
    }

    public void Aim(bool active)
    {
        Dispatch("AIM", data =>
        {
            data.active = active;
        });
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    void Dispatch(string commandType, System.Action<CommandData> configureData)
    {
        AgentCommand cmd = BuildCommand(commandType, configureData);
        if (CommandDispatcher.Instance != null)
            CommandDispatcher.Instance.Dispatch(cmd);
        else
            Debug.LogWarning("[DebugConsole] CommandDispatcher not found in scene.");
    }

    AgentCommand BuildCommand(string commandType, System.Action<CommandData> configureData)
    {
        var data = new CommandData();
        configureData?.Invoke(data);
        return new AgentCommand
        {
            commandType = commandType,
            agentId     = "DebugConsole",
            timestamp   = Time.time,
            data        = data
        };
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // ------------------------------------------------------------------
    // Minimal in-Editor GUI panel (no keyboard shortcuts)
    // ------------------------------------------------------------------

    bool _showPanel = true;

    void OnGUI()
    {
        if (!_showPanel) return;

        GUILayout.BeginArea(new Rect(10, 10, 220, 320), GUI.skin.box);
        GUILayout.Label("DebugConsole");

        if (GUILayout.Button("Move Forward"))  Move(0f, 1f);
        if (GUILayout.Button("Move Backward")) Move(0f, -1f);
        if (GUILayout.Button("Move Left"))     Move(-1f, 0f);
        if (GUILayout.Button("Move Right"))    Move(1f, 0f);
        if (GUILayout.Button("Stop"))          Move(0f, 0f);

        GUILayout.Space(4);
        if (GUILayout.Button("Shoot (press)"))   Shoot(true);
        if (GUILayout.Button("Shoot (release)")) Shoot(false);
        if (GUILayout.Button("Reload"))          Reload();
        if (GUILayout.Button("Aim ON"))          Aim(true);
        if (GUILayout.Button("Aim OFF"))         Aim(false);

        GUILayout.Space(4);
        if (GUILayout.Button("Weapon 0 (Rifle)"))  SwitchWeapon(0);
        if (GUILayout.Button("Weapon 1 (Sniper)")) SwitchWeapon(1);
        if (GUILayout.Button("Weapon 2 (Pistol)")) SwitchWeapon(2);

        GUILayout.Space(4);
        if (GUILayout.Button("Hide Panel")) _showPanel = false;

        GUILayout.EndArea();
    }
#endif
}
