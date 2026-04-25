using UnityEngine;

namespace PlayerAssets
{
    // Input state container. All fields are set programmatically via
    // CommandDispatcher -> PlayerCommandAPI -> AIInputFeeder.
    // Human keyboard/mouse/gamepad input is intentionally removed.
    public class PlayerAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool aim;
        public bool shoot;
        public bool rightSlash;
        public bool reload;
        public bool openInventory;
        public bool openScoreboard;
        public bool interact;
        public bool hotkey1;
        public bool hotkey2;
        public bool hotkey3;
        public bool hotkey4;
        public bool hotkey5;
        public bool escapeUI;

        [Header("Movement Settings")]
        public bool analogMovement;

        // Programmatic setters — called by AIInputFeeder / PlayerCommandAPI
        public void MoveInput(Vector2 newMoveDirection)       { move = newMoveDirection; }
        public void LookInput(Vector2 newLookDirection)       { look = newLookDirection; }
        public void JumpInput(bool newJumpState)              { jump = newJumpState; }
        public void SprintInput(bool newSprintState)          { sprint = newSprintState; }
        public void AimInput(bool newAimState)                { aim = newAimState; }
        public void ShootInput(bool newShootState)            { shoot = newShootState; }
        public void RightSlashInput(bool newRightSlashState)  { rightSlash = newRightSlashState; }
        public void ReloadInput(bool newReloadState)          { reload = newReloadState; }
        public void InteractInput(bool newInteractState)      { interact = newInteractState; }
        public void OpenInventoryInput(bool v)                { openInventory = v; }
        public void OpenScoreboardInput(bool v)               { openScoreboard = v; }
        public void EscapeUIInput(bool v)                     { escapeUI = v; }
        public void Hotkey1Input(bool v)                      { hotkey1 = v; }
        public void Hotkey2Input(bool v)                      { hotkey2 = v; }
        public void Hotkey3Input(bool v)                      { hotkey3 = v; }
        public void Hotkey4Input(bool v)                      { hotkey4 = v; }
        public void Hotkey5Input(bool v)                      { hotkey5 = v; }
    }
}
