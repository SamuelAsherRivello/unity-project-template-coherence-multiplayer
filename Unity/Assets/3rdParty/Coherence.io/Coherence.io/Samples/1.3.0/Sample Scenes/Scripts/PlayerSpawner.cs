using System.Collections.Generic;
using Coherence.Connection;
using Coherence.Toolkit;
using UnityEngine;

namespace Coherence.Samples.PlayerSpawner
{
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField]
        private CoherenceSync prefabToSpawn;

        private CoherenceSync prefabInstance;
        private CoherenceBridge coherenceBridge;
        private CoherenceSync coherenceSync;
        private Queue<Transform> unusedSpawnPoints;
        private HashSet<ClientID> assignedClients;

        private void Awake()
        {
            CoherenceBridgeStore.TryGetBridge(gameObject.scene, out coherenceBridge);
            coherenceBridge.onDisconnected.AddListener(OnDisconnected);
            coherenceSync = GetComponent<CoherenceSync>();
        }

        public void SpawnPrefab(uint clientId, Vector3 worldPosition)
        {
            if ((uint)coherenceBridge.ClientID != clientId)
            {
                return;
            }

            prefabInstance = Instantiate(prefabToSpawn, worldPosition, Quaternion.identity);
        }

        public void Init(List<Transform> spawnPoints)
        {
            unusedSpawnPoints = new Queue<Transform>(spawnPoints);
            assignedClients = new HashSet<ClientID>();

            coherenceBridge.ClientConnections.OnCreated += AssignSpawnPointToClient;
            Vector3 spawnPoint = GetSpawnPointPosition();
            SpawnPrefab((uint)coherenceBridge.ClientID, spawnPoint);

            AssignSpawnPointToClients();
        }

        private void AssignSpawnPointToClients()
        {
            foreach (CoherenceClientConnection clientConnection in coherenceBridge.ClientConnections.GetAll())
            {
                AssignSpawnPointToClient(clientConnection);
            }
        }

        private void AssignSpawnPointToClient(CoherenceClientConnection clientConnection)
        {
            ClientID clientID = clientConnection.ClientId;

            bool hasIndexAssigned = assignedClients.Contains(clientID);
            if (hasIndexAssigned) return;

            Vector3 spawnPosition = GetSpawnPointPosition();
            coherenceSync.SendCommand(SpawnPrefab, MessageTarget.Other, (uint)clientID, spawnPosition);

            assignedClients.Add(clientID);
        }

        private Vector3 GetSpawnPointPosition()
        {
            if (unusedSpawnPoints.Count == 0)
            {
                Debug.LogWarning($"No more spawn points available! Assigning {Vector3.zero}");
                return Vector3.zero;
            }

            Transform spawnPoint = unusedSpawnPoints.Dequeue();
            return spawnPoint.transform.position;
        }

        private void OnDisconnected(CoherenceBridge _, ConnectionCloseReason __)
        {
            if (prefabInstance != null)
            {
                Destroy(prefabInstance.gameObject);
            }

            assignedClients = null;
            unusedSpawnPoints = null;
            coherenceBridge.ClientConnections.OnCreated -= AssignSpawnPointToClient;
        }
    }
}
