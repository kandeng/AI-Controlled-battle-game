using System;
using UnityEngine;

public class AIInputFeeder : PlayerBehaviour
{
    public Vector3 moveDir;
    public Vector3 lookEuler;

    // Actions invoked by PlayerCommandAPI / CommandDispatcher
    public Action<Vector3> OnMove;
    public Action<Vector3> OnLook;
    public Action<bool>    OnAttack;
    public Action<bool>    OnReload;
    public Action<int>     OnSwitchWeapon;
    public Action<bool>    OnAim;

    public override void InitializeStart()
    {
        base.InitializeStart();
        OnMove += (val) =>
        {
            moveDir = val;
            // Also drive the PlayerAssetsInputs path used by non-bot PlayerController.Move()
            PlayerRoot.PlayerAssetsInputs.MoveInput(new Vector2(val.x, val.z));
        };

        OnLook += (val) =>
        {
            lookEuler = val;
            // Also drive the PlayerAssetsInputs path used by non-bot CameraRotation()
            PlayerRoot.PlayerAssetsInputs.LookInput(new Vector2(val.y, val.x));
        };

        OnAttack += (val) =>
        {
            PlayerRoot.PlayerAssetsInputs.ShootInput(val);
        };

        OnReload += (val) =>
        {
            PlayerRoot.PlayerAssetsInputs.ReloadInput(val);
        };

        OnSwitchWeapon += (slot) =>
        {
            // Clear all hotkeys, then set the requested one
            PlayerRoot.PlayerAssetsInputs.Hotkey1Input(false);
            PlayerRoot.PlayerAssetsInputs.Hotkey2Input(false);
            PlayerRoot.PlayerAssetsInputs.Hotkey3Input(false);
            PlayerRoot.PlayerAssetsInputs.Hotkey4Input(false);
            PlayerRoot.PlayerAssetsInputs.Hotkey5Input(false);

            switch (slot)
            {
                case 0: PlayerRoot.PlayerAssetsInputs.Hotkey1Input(true); break;
                case 1: PlayerRoot.PlayerAssetsInputs.Hotkey2Input(true); break;
                case 2: PlayerRoot.PlayerAssetsInputs.Hotkey3Input(true); break;
                case 3: PlayerRoot.PlayerAssetsInputs.Hotkey4Input(true); break;
                case 4: PlayerRoot.PlayerAssetsInputs.Hotkey5Input(true); break;
            }
        };

        OnAim += (val) =>
        {
            PlayerRoot.PlayerAssetsInputs.AimInput(val);
        };
    }
}
