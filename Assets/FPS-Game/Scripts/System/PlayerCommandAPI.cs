using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implements IPlayerCommandAPI by delegating to AIInputFeeder.
/// Supports agentId-based player targeting for multi-agent scenarios.
/// Attach to the same GameObject as CommandDispatcher.
/// </summary>
public class PlayerCommandAPI : MonoBehaviour, IPlayerCommandAPI
{
    public static PlayerCommandAPI Instance { get; private set; }

    // Maps agentId → PlayerRoot. Populated on first use when a script announces its agentId.
    // Key "" (empty) always falls back to the first non-bot PlayerRoot in the scene.
    private readonly Dictionary<string, PlayerRoot> _agentPlayerMap = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // -------------------------------------------------------
    // Agent → Player binding
    // -------------------------------------------------------

    /// <summary>
    /// Bind an agentId to a specific PlayerRoot.
    /// Called by CommandDispatcher when it processes the first command from a new agent.
    /// </summary>
    public void BindAgent(string agentId, PlayerRoot player)
    {
        if (string.IsNullOrEmpty(agentId) || player == null) return;
        _agentPlayerMap[agentId] = player;
        Debug.Log($"[PlayerCommandAPI] Agent '{agentId}' bound to player '{player.gameObject.name}'");
    }

    /// <summary>
    /// Resolve the PlayerRoot for an agentId.
    /// Assignment order: explicit binding → round-robin free player → first non-bot player.
    /// </summary>
    public PlayerRoot ResolvePlayer(string agentId)
    {
        // Explicit binding
        if (!string.IsNullOrEmpty(agentId) && _agentPlayerMap.TryGetValue(agentId, out var bound))
        {
            return bound;
        }

        // Auto-assign: find a non-bot player not yet bound to any agent
        var allPlayers = new List<PlayerRoot>(FindObjectsByType<PlayerRoot>(FindObjectsInactive.Exclude));
        var boundPlayers = new HashSet<PlayerRoot>(_agentPlayerMap.Values);

        foreach (var p in allPlayers)
        {
            if (!p.IsCharacterBot() && !boundPlayers.Contains(p))
            {
                if (!string.IsNullOrEmpty(agentId))
                    BindAgent(agentId, p);
                return p;
            }
        }

        // Fallback: first non-bot player regardless of binding
        foreach (var p in allPlayers)
            if (!p.IsCharacterBot()) { return p; }

        Debug.LogWarning($"[PlayerCommandAPI] ResolvePlayer '{agentId}': NO non-bot PlayerRoot found!");
        return null;
    }

    // -------------------------------------------------------
    // IPlayerCommandAPI — single-agent convenience (uses first free player)
    // -------------------------------------------------------

    PlayerRoot GetPlayer() => ResolvePlayer("");

    public void Move(Vector2 direction)          => MoveFor("", direction);
    public void Look(float pitch, float yaw)     => LookFor("", pitch, yaw);
    public void Shoot(bool pressed)              => ShootFor("", pressed);
    public void Reload()                         => ReloadFor("");
    public void SwitchWeapon(int slotIndex)      => SwitchWeaponFor("", slotIndex);
    public void Aim(bool active)                 => AimFor("", active);

    // -------------------------------------------------------
    // Multi-agent variants (keyed by agentId)
    // -------------------------------------------------------

    public void MoveFor(string agentId, Vector2 direction)
    {
        PlayerRoot player = ResolvePlayer(agentId);
        if (player == null) { Debug.LogWarning($"[PlayerCommandAPI] MoveFor: no player for agentId='{agentId}'"); return; }
        player.AIInputFeeder.OnMove?.Invoke(new Vector3(direction.x, 0f, direction.y));
    }

    public void LookFor(string agentId, float pitch, float yaw)
    {
        PlayerRoot player = ResolvePlayer(agentId);
        if (player == null) return;
        player.AIInputFeeder.OnLook?.Invoke(new Vector3(pitch, yaw, 0f));
    }

    public void ShootFor(string agentId, bool pressed)
    {
        PlayerRoot player = ResolvePlayer(agentId);
        if (player == null) return;
        player.AIInputFeeder.OnAttack?.Invoke(pressed);
    }

    public void ReloadFor(string agentId)
    {
        PlayerRoot player = ResolvePlayer(agentId);
        if (player == null) return;
        player.AIInputFeeder.OnReload?.Invoke(true);
        StartCoroutine(ResetReloadNextFrame(player));
    }

    System.Collections.IEnumerator ResetReloadNextFrame(PlayerRoot player)
    {
        yield return null;
        player.AIInputFeeder.OnReload?.Invoke(false);
    }

    public void SwitchWeaponFor(string agentId, int slotIndex)
    {
        PlayerRoot player = ResolvePlayer(agentId);
        if (player == null) return;
        player.AIInputFeeder.OnSwitchWeapon?.Invoke(slotIndex);
    }

    public void AimFor(string agentId, bool active)
    {
        PlayerRoot player = ResolvePlayer(agentId);
        if (player == null) return;
        player.AIInputFeeder.OnAim?.Invoke(active);
    }

    // -------------------------------------------------------
    // Camera view control
    // -------------------------------------------------------

    /// <summary>
    /// Point the Cinemachine follow camera at the player bound to targetAgentId.
    /// If targetAgentId is empty, follows the agent that issued the command (sourceAgentId).
    /// </summary>
    public void SetView(string sourceAgentId, string targetAgentId)
    {
        string resolveId = string.IsNullOrEmpty(targetAgentId) ? sourceAgentId : targetAgentId;
        PlayerRoot player = ResolvePlayer(resolveId);
        if (player == null)
        {
            Debug.LogWarning($"[PlayerCommandAPI] SetView: no player found for agentId='{resolveId}'");
            return;
        }

        SetViewToPlayer(player);
    }

    /// <summary>
    /// Point the Cinemachine follow camera at an explicit PlayerRoot.
    /// </summary>
    public void SetViewToPlayer(PlayerRoot player)
    {
        if (InGameManager.Instance == null) return;
        var cam = InGameManager.Instance.PlayerFollowCamera;
        if (cam == null) return;

        Transform target = player.PlayerCamera.GetPlayerCameraTarget();
        if (target != null)
        {
            cam.Follow = target;
            cam.LookAt = target;
            Debug.Log($"[PlayerCommandAPI] View set to player '{player.gameObject.name}'");
        }

        // Notify UI selector
        ViewSelectorUI.Instance?.RefreshSelection(player);
    }
}
