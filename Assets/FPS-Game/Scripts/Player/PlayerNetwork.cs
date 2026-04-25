using Cinemachine;
using Unity.Netcode;
using UnityEngine;
// Removed: using Unity.Services.Lobbies.Models;
// Removed: using Unity.Services.Authentication;
using System.Collections.Generic;
using System;
using System.Collections;
using Unity.Collections;
using UnityEngine.AI;

public class PlayerNetwork : PlayerBehaviour
{
    public string playerName = "Playername";
    public NetworkVariable<int> KillCount = new(0);
    public NetworkVariable<int> DeathCount = new(0);

    public float RespawnDelay;

    // OnNetworkSpawn
    public override int PriorityNetwork => 5;
    public override void InitializeOnNetworkSpawn()
    {
        base.InitializeOnNetworkSpawn();
        if (IsOwner)
        {
            if (IsServer && gameObject.name.Contains("Bot"))
            {
                string botID = gameObject.name.Replace("Bot#", ""); // Xóa tiền tố "Bot#"
                PlayerRoot.SetIsCharacterBot(true);
                PlayerRoot.BotID.Value = botID;
                Debug.Log($"Bot id: {PlayerRoot.GetBotID()}");
            }

            EnableScripts();
            if (!PlayerRoot.IsCharacterBot())
            {
                // Removed: Authentication-based player name mapping
                MappingValues_ServerRpc($"Player_{OwnerClientId}", OwnerClientId);
                PlayerRoot.PlayerModel.ChangeModelVisibility(false);

                gameObject.name += " Local";
                GetComponent<NavMeshAgent>().enabled = false;
            }
            PlayerRoot.Events.OnPlayerDead += OnPlayerDead;
            PlayerRoot.Events.OnPlayerRespawn += OnPlayerRespawn;
        }
        else
        {
            if (PlayerRoot.IsCharacterBot())
            {
                SyncBotNetwork();
            }
        }
    }

    void OnDisable()
    {
        PlayerRoot.Events.OnPlayerDead -= OnPlayerDead;
        PlayerRoot.Events.OnPlayerRespawn -= OnPlayerRespawn;
    }

    public override void OnInGameManagerReady(InGameManager manager)
    {
        base.OnInGameManagerReady(manager);
        if (IsOwner)
        {
            StartCoroutine(SpawnRandom(() =>
            {
                if (!gameObject.name.Contains("Bot")) SetCinemachineVirtualCamera();
            }));
        }

        if (!gameObject.name.Contains("Bot"))
        {
            InGameManager.Instance.AllCharacters.Add(gameObject.GetComponent<PlayerRoot>());
        }
    }

    void SetCinemachineVirtualCamera()
    {
        CinemachineVirtualCamera _camera = InGameManager.Instance.PlayerFollowCamera;
        if (_camera != null)
        {
            Transform playerCamera = PlayerRoot.PlayerCamera.GetPlayerCameraTarget();

            if (playerCamera != null) _camera.Follow = playerCamera;
            if (_camera.Follow == null) Debug.Log("_camera.Follow = null");
        }
    }

    void RemoveCinemachineVirtualCamera()
    {
        CinemachineVirtualCamera _camera = InGameManager.Instance.PlayerFollowCamera;
        if (_camera != null)
        {
            Transform playerCameraRoot = transform.Find("PlayerCameraRoot");

            if (playerCameraRoot != null) _camera.Follow = null;
        }
    }

    void SyncBotNetwork()
    {
        SyncBotNetwork_InspectorName();
        SyncBotNetwork_Component();
    }

    void SyncBotNetwork_InspectorName()
    {
        gameObject.name = "Bot#" + PlayerRoot.BotID.Value.ToString();
    }

    void SyncBotNetwork_Component()
    {
        PlayerRoot.PlayerCamera.enabled = false;
        // PlayerRoot.PlayerModel.ChangeRigBuilderState(false);
    }

    // Local
    IEnumerator SpawnRandom(Action onSpawnComplete = null)
    {
        yield return null;
        SpawnPosition randomPos = InGameManager.Instance.RandomSpawn.GetRandomPos();

        Debug.Log($"Spawn at {randomPos.gameObject.name}: {randomPos.SpawnPos} {randomPos.SpawnRot.eulerAngles}");
        transform.position = randomPos.SpawnPos;
        PlayerRoot.PlayerController.RotateCameraTo(randomPos.SpawnRot);
        onSpawnComplete?.Invoke();
    }

    void SetRandomPos()
    {
        StartCoroutine(SpawnRandom(() =>
        {
            ToggleCharacterState(true);
            if (!PlayerRoot.IsCharacterBot())
                PlayerRoot.PlayerUI.CurrentPlayerCanvas.HitEffect.ResetHitEffect();
        }));
    }

    // Hàm được gọi khi event OnPlayerDead được kích hoạt ở local (có được từ tín hiệu ở hàm OnHPChanged được cập nhật tự động từ mạng)
    void OnPlayerDead()
    {
        if (!PlayerRoot.IsCharacterBot())
            RemoveCinemachineVirtualCamera();

        ToggleCharacterState(false);
        Invoke(nameof(Respawn), RespawnDelay);
        Invoke(nameof(SetRandomPos), RespawnDelay);
    }

    void Respawn()
    {
        PlayerRoot.Events.InvokeOnPlayerRespawn();
    }

    void OnPlayerRespawn()
    {
        if (!PlayerRoot.IsCharacterBot())
            SetCinemachineVirtualCamera();
    }

    void EnableScripts()
    {
        PlayerRoot.CharacterController.enabled = true;
        PlayerRoot.PlayerController.enabled = true;
        PlayerRoot.PlayerShoot.enabled = true;

        if (!PlayerRoot.IsCharacterBot())
        {
            PlayerRoot.PlayerUI.enabled = true;
        }
        else
        {
            PlayerRoot.PlayerUI.enabled = false;
            PlayerRoot.PlayerCamera.enabled = false;
        }
    }

    // Simplified: No longer uses Lobby system.
    // Sets the player name directly on this instance (owner) AND syncs to server.
    // Falls back to self-assignment when PlayerObject is not yet registered
    // (race condition on host during OnNetworkSpawn).
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void MappingValues_ServerRpc(string playerID, ulong targetClientId)
    {
        // Try via ConnectedClients first (normal multiplayer path)
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(targetClientId, out var client)
            && client.PlayerObject != null
            && client.PlayerObject.TryGetComponent<PlayerNetwork>(out var playerNetwork))
        {
            playerNetwork.playerName = playerID;
            Debug.Log($"[PlayerNetwork] Set player name via ConnectedClients: {playerID}");
            return;
        }

        // Fallback: PlayerObject not yet registered (host Single-Player spawn race).
        // Apply directly to this NetworkBehaviour since we ARE the target.
        if (OwnerClientId == targetClientId)
        {
            playerName = playerID;
            Debug.Log($"[PlayerNetwork] Set player name (direct fallback): {playerID}");
        }
    }

    void ToggleCharacterState(bool isActive)
    {
        PlayerRoot.CharacterController.enabled = isActive;
        PlayerRoot.PlayerController.enabled = isActive;
    }

}
