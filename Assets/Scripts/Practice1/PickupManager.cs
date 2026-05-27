using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

namespace Practice1
{
    public class PickupManager : MonoBehaviour
    {
        [SerializeField] private GameObject _healthPickupPrefab;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private float _respawnDelay = 10f;
        private readonly List<Coroutine> _respawnCoroutines = new();

        private void OnEnable()
        {
            if (InstanceFinder.NetworkManager != null)
            {
                InstanceFinder.NetworkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
            }
        }

        private void OnDisable()
        {
            if (InstanceFinder.NetworkManager != null)
            {
                InstanceFinder.NetworkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
            }
            for (int i = 0; i < _respawnCoroutines.Count; i++)
            {
                if (_respawnCoroutines[i] != null)
                    StopCoroutine(_respawnCoroutines[i]);
            }
            _respawnCoroutines.Clear();
        }

        private void OnServerConnectionState(ServerConnectionStateArgs args)
        {
            if (args.ConnectionState != LocalConnectionState.Stopped)
            {
                return;
            }

            ClearExistingPickups();
            for (int i = 0; i < _respawnCoroutines.Count; i++)
            {
                if (_respawnCoroutines[i] != null)
                    StopCoroutine(_respawnCoroutines[i]);
            }
            _respawnCoroutines.Clear();
        }

        [Server]
        public void SpawnRoundPickups()
        {
            if (!InstanceFinder.IsServerStarted)
            {
                return;
            }

            ClearExistingPickups();
            SpawnAll();
        }

        [Server]
        private void SpawnAll()
        {
            if (_spawnPoints == null || _spawnPoints.Length == 0)
            {
                Debug.LogWarning("[PickupManager] Spawn points are not configured.");
                return;
            }

            for (int i = 0; i < _spawnPoints.Length; i++)
            {
                if (_spawnPoints[i] != null)
                {
                    SpawnPickup(_spawnPoints[i].position);
                }
            }
        }

        public void OnPickedUp(Vector3 position)
        {
            if (!InstanceFinder.IsServerStarted)
            {
                return;
            }

            Coroutine routine = StartCoroutine(RespawnAfterDelay(position));
            _respawnCoroutines.Add(routine);
        }

        private IEnumerator RespawnAfterDelay(Vector3 position)
        {
            yield return new WaitForSeconds(_respawnDelay);
            SpawnPickup(position);
        }

        private void SpawnPickup(Vector3 position)
        {
            if (_healthPickupPrefab == null)
            {
                Debug.LogError("[PickupManager] Health pickup prefab is not assigned.");
                return;
            }

            GameObject pickup = Instantiate(_healthPickupPrefab, position, Quaternion.identity);
            HealthPickup healthPickup = pickup.GetComponent<HealthPickup>();
            if (healthPickup != null)
            {
                healthPickup.Init(this);
            }

            NetworkObject networkObject = pickup.GetComponent<NetworkObject>();
            if (networkObject != null && InstanceFinder.ServerManager != null)
            {
                InstanceFinder.ServerManager.Spawn(networkObject);
            }
            else
            {
                Debug.LogError("[PickupManager] Spawn failed: NetworkObject or ServerManager is missing.");
            }
        }

        [Server]
        private static void ClearExistingPickups()
        {
            HealthPickup[] pickups = FindObjectsByType<HealthPickup>(FindObjectsSortMode.None);
            for (int i = 0; i < pickups.Length; i++)
            {
                if (pickups[i] != null && pickups[i].IsSpawned)
                {
                    pickups[i].Despawn();
                }
            }
        }
    }
}
