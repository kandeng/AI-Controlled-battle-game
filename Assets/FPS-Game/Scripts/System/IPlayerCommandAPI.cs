using UnityEngine;

/// <summary>
/// Clean API surface for controlling the player character.
/// Implementations: PlayerCommandAPI.
/// Callers: CommandDispatcher (via WebSocket or DebugConsole).
/// </summary>
public interface IPlayerCommandAPI
{
    /// <summary>Move in the XZ plane. direction is normalized (x = strafe, y = forward).</summary>
    void Move(Vector2 direction);

    /// <summary>Set camera look target. pitch = vertical degrees, yaw = horizontal degrees.</summary>
    void Look(float pitch, float yaw);

    /// <summary>Press (true) or release (false) the fire trigger.</summary>
    void Shoot(bool pressed);

    /// <summary>Trigger a reload.</summary>
    void Reload();

    /// <summary>Switch to weapon slot (0-based).</summary>
    void SwitchWeapon(int slotIndex);

    /// <summary>Enter (true) or exit (false) aim-down-sights.</summary>
    void Aim(bool active);
}
