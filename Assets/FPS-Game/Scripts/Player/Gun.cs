using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System;

public class Gun : PlayerBehaviour
{
    public SwayAndBob SwayAndBob { get; private set; }

    SupplyLoad _supplyLoad;
    private bool isPressed = false;
    public float FireCoolDown;
    public float ReloadCoolDown;

    [SerializeField] _ShootEffect shootEffect;
    [SerializeField] float _aimFOV;

    [SerializeField] float _spreadAngle;

    [SerializeField] AudioSource gunSound;

    // public Image crossHair;

    public bool Automatic;

    // private float CurrentCoolDown;
    bool isReadyToFire = true;

    private float nextFireTime;

    // [SerializeField] private Magazine magazine;

    //private float delayTime = 1f;
    //private float counter = 0f;

    // [SerializeField] private GameObject bulletSpawnPoint;
    //[SerializeField]
    //private Transform orientation;
    // [SerializeField] private float fireRate;
    // [SerializeField] private float speed;

    [Header("Damage")]
    [SerializeField] float _headDamage;
    [SerializeField] float _torsoDamage;
    [SerializeField] float _legDamage;

    [Header("Gun type")]
    [SerializeField] GunType _gunType;
    [SerializeField] float _aimTransitionDuration;

    public float HeadDamage { get; private set; }
    public float TorsoDamage { get; private set; }
    public float LegDamage { get; private set; }

    public float GetAimFOV() { return _aimFOV; }

    public override void InitializeAwake()
    {
        base.InitializeAwake();
        SwayAndBob = GetComponent<SwayAndBob>();

        HeadDamage = _headDamage;
        TorsoDamage = _torsoDamage;
        LegDamage = _legDamage;

        // CurrentCoolDown = FireCoolDown;
    }

    public override void InitializeStart()
    {
        base.InitializeStart();
        _supplyLoad = GetComponent<SupplyLoad>();
    }

    void OnEnable()
    {
        PlayerRoot.Events.OnAimStateChanged += HandleAimStateChanged;

        gunSound.spatialBlend = 1f;
        gunSound.maxDistance = 100f;
    }

    void OnDisable()
    {
        PlayerRoot.Events.OnAimStateChanged -= HandleAimStateChanged;
    }

    void HandleAimStateChanged(bool isAim)
    {
        if (isAim)
        {
            if (PlayerRoot.WeaponHolder.WeaponPoseLocalSOs[_gunType].TryGetPose(PlayerWeaponPose.Aim, out var data))
            {
                StartCoroutine(TransitionAimState(data.Position, data.EulerRotation));
            }
        }
        else
        {
            if (PlayerRoot.WeaponHolder.WeaponPoseLocalSOs[_gunType].TryGetPose(PlayerWeaponPose.Idle, out var data))
            {
                StartCoroutine(TransitionAimState(data.Position, data.EulerRotation));
            }
        }
    }

    private void Shoot()
    {
        if (_supplyLoad.IsMagazineEmpty()) return;
        if (PlayerRoot.PlayerReload.IsReloading) return;

        if (PlayerRoot.PlayerAssetsInputs.shoot == false) isPressed = false;
        if (PlayerRoot.PlayerAssetsInputs.shoot == true)
        {
            if (Automatic)
            {
                if (!isReadyToFire) return;
                PlayerRoot.Events.InvokeOnGunShoot();
                PlayerRoot.PlayerInventory.UpdatecurrentMagazineAmmo();
                PlayerRoot.PlayerShoot.Shoot(_spreadAngle, _gunType);

                shootEffect.ActiveShootEffect();

                if (gameObject.tag == "AK47")
                {
                    if (Time.time >= nextFireTime)
                    {
                        PlayGunAudio_ServerRpc(transform.position);
                        nextFireTime = Time.time + FireCoolDown;
                    }
                }

                isReadyToFire = false;
                StartTimer(FireCoolDown, () =>
                {
                    PlayerRoot.Events.InvokeOnDoneGunShoot();
                    isReadyToFire = true;
                });
            }
            else
            {
                if (!isReadyToFire) return;

                if (isPressed == false)
                {
                    isPressed = true;
                    PlayerRoot.Events.InvokeOnGunShoot();

                    PlayerRoot.PlayerInventory.UpdatecurrentMagazineAmmo();
                    PlayerRoot.PlayerShoot.Shoot(_spreadAngle, _gunType);

                    shootEffect.ActiveShootEffect();

                    if (gameObject.tag == "Sniper")
                    {
                        if (Time.time >= nextFireTime)
                        {
                            StopGunAudio_ServerRpc(transform.position);
                            PlayGunAudio_ServerRpc(transform.position);
                        }
                    }

                    if (gameObject.tag == "Pistol")
                    {
                        if (Time.time >= nextFireTime)
                        {
                            StopGunAudio_ServerRpc(transform.position);
                            PlayGunAudio_ServerRpc(transform.position);
                        }
                    }

                    isReadyToFire = false;
                    StartTimer(FireCoolDown, () =>
                    {
                        PlayerRoot.Events.InvokeOnDoneGunShoot();
                        isReadyToFire = true;
                    });
                }
            }
        }

        // if (Automatic)
        // { ... old duplicate block removed }
    }

    public IEnumerator TransitionAimState(Vector3 targetPos, Vector3 targetEulerRot)
    {
        Vector3 originPos = transform.localPosition;
        Vector3 originEulerRot = transform.localEulerAngles;

        if (originPos == targetPos && originEulerRot == targetEulerRot)
            yield break;

        SwayAndBob.enabled = false;

        Quaternion originRot = Quaternion.Euler(originEulerRot);
        Quaternion targetRot = Quaternion.Euler(targetEulerRot);

        float elapsedTime = 0f;

        while (elapsedTime < _aimTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / _aimTransitionDuration);

            Vector3 newPos = Vector3.Lerp(originPos, targetPos, t);
            Quaternion newRot = Quaternion.Slerp(originRot, targetRot, t);

            transform.SetLocalPositionAndRotation(newPos, newRot);

            yield return null;
        }

        transform.localPosition = targetPos;
        transform.localEulerAngles = targetEulerRot;

        SwayAndBob.enabled = true;
        SwayAndBob.AimPositionOffset = transform.localPosition;
        SwayAndBob.AimRotationOffset = Quaternion.Euler(transform.localEulerAngles);
    }


    void Aim() { /* transition handled by HandleAimStateChanged via event */ }

    public void PlayGunAudio(Vector3 position)
    {
        gunSound.transform.position = position;
        gunSound.Play();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void PlayGunAudio_ServerRpc(Vector3 position)
    {
        PlayGunAudio(position);
        PlayGunAudio_ClientRpc(position);
    }

    [ClientRpc]
    public void PlayGunAudio_ClientRpc(Vector3 position)
    {
        PlayGunAudio(position);
    }

    public void StopGunAudio(Vector3 position)
    {
        gunSound.transform.position = position;
        gunSound.Stop();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void StopGunAudio_ServerRpc(Vector3 position)
    {
        StopGunAudio(position);
        StopGunAudio_ClientRpc(position);
    }

    [ClientRpc]
    public void StopGunAudio_ClientRpc(Vector3 position)
    {
        StopGunAudio(position);
    }

    // dead audio/aim commented blocks removed

    private void Update()
    {
        if (IsOwner == false) return;
        if (PlayerRoot.PlayerTakeDamage.IsPlayerDead()) return;

        Shoot();
        Aim();

        Tick(Time.deltaTime);
    }

    #region Timer
    bool IsRunning = false;
    float RemainingTime;
    Action onFinishedTimer;

    void StartTimer(float duration, Action onFinished = null)
    {
        if (IsRunning == true) return;

        RemainingTime = duration;
        IsRunning = true;
        onFinishedTimer = onFinished;
    }

    void Tick(float deltaTime)
    {
        if (!IsRunning) return;

        RemainingTime -= deltaTime;

        if (RemainingTime <= 0f)
        {
            RemainingTime = 0f;
            IsRunning = false;
            onFinishedTimer?.Invoke();
        }
    }
    #endregion
}