using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnInGameManager : MonoBehaviour
{
    [SerializeField] private GameObject inGameManagerPrefab;
    [SerializeField] Transform spawnPositions;
    [SerializeField] Transform waypoints;
    [SerializeField] ZonesContainer zonesContainer;
    [SerializeField] ZonePortalsContainer zonesPortalContainer;
    [SerializeField] TacticalPoints tacticalPoints;

    public Transform GetSpawnPositions() { return spawnPositions; }
    public Transform GetWaypoints() { return waypoints; }
    public ZonesContainer GetZonesContainer() { return zonesContainer; }
    public ZonePortalsContainer GetZonePortalsContainer() { return zonesPortalContainer; }
    public List<Transform> GetTacticalPointsList() { return tacticalPoints.TPoints; }

    void Awake()
    {
        // If NetworkManager is already initialized and listening, spawn immediately
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            TrySpawn();
        }
        else if (NetworkManager.Singleton != null)
        {
            // NetworkManager exists but not listening yet - subscribe to event
            NetworkManager.Singleton.OnServerStarted += TrySpawn;
        }
        else
        {
            // NetworkManager not yet initialized - will be created by InGameManager
            // This is normal when running Play.unity directly in Editor
            Debug.Log("[SpawnInGameManager] NetworkManager not ready yet - will spawn when InGameManager initializes");
        }
    }

    private void TrySpawn()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        if (InGameManager.Instance != null)
        {
            Debug.Log("[SpawnInGameManager] Instance đã tồn tại, bỏ qua spawn.");
            return;
        }

        if (inGameManagerPrefab == null)
        {
            Debug.LogError("[SpawnInGameManager] Prefab chưa gán!");
            return;
        }

        var obj = Instantiate(inGameManagerPrefab);
        var netObj = obj.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("[SpawnInGameManager] Prefab thiếu NetworkObject!");
            Destroy(obj);
            return;
        }

        netObj.Spawn();
        Debug.Log("[SpawnInGameManager] Spawned InGameManager sớm bằng OnServerStarted.");
    }
}