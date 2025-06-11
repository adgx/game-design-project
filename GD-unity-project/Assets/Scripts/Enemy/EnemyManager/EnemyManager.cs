using System.Collections.Generic;
using System.Linq;
using RoomManager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy.EnemyManager
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }

        public List<EnemyData.EnemyData> availableEnemyTypes;

        private int minEnemiesPerRoom = 1;
        private int maxEnemiesPerRoom = 5;

        private RoomManager.RoomManager roomManager;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }

            if (availableEnemyTypes == null || availableEnemyTypes.Count == 0)
            {
                Debug.LogError(
                    "EnemyManager: 'Available Enemy Types' list is not assigned or is empty! Cannot spawn enemies.",
                    this);
                enabled = false;
            }
        }

        private void Start()
        {
            roomManager = RoomManager.RoomManager.Instance;

            if (roomManager)
            {
                roomManager.PlayerEnteredNewRoom += HandlePlayerEnteredNewRoom;
            }
            else
            {
                Debug.LogError("EnemyManager: Could not find RoomManager.Instance! Enemy spawning will not occur.",
                    this);
                enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (roomManager)
            {
                roomManager.PlayerEnteredNewRoom -= HandlePlayerEnteredNewRoom;
            }
        }

        private void HandlePlayerEnteredNewRoom(Vector3Int newRoomIndex)
        {
            if (!roomManager) return;

            Room currentRoom = roomManager.GetRoomByGridIndex(newRoomIndex);

            if (currentRoom.hasEnemiesSpawned || currentRoom.enemySpawnPoints.Count == 0) return;

            int currentRoomBudget = currentRoom.maxSpawnCost;
            int enemiesSpawnedThisRoom = 0;

            List<Transform> roomSpawnPoints = currentRoom.enemySpawnPoints
                .Where(sp => sp)
                .OrderBy(sp => Random.value)
                .ToList();

            for (int i = 0; i < maxEnemiesPerRoom && roomSpawnPoints.Count > 0 && currentRoomBudget > 0; i++)
            {
                List<EnemyData.EnemyData> affordableEnemyTypes = availableEnemyTypes
                    .Where(et => et && et.enemyPrefab && et.spawnCost > 0 && et.spawnCost <= currentRoomBudget)
                    .OrderBy(et => et.spawnCost)
                    .ToList();

                if (affordableEnemyTypes.Count == 0) break;

                EnemyData.EnemyData enemyToSpawnData = enemiesSpawnedThisRoom < minEnemiesPerRoom
                    ? affordableEnemyTypes.FirstOrDefault()
                    : affordableEnemyTypes[Random.Range(0, affordableEnemyTypes.Count)];

                if (!enemyToSpawnData) return;

                Transform spawnPoint = roomSpawnPoints[0];
                roomSpawnPoints.RemoveAt(0);

                GameObject enemyInstance =
                    Instantiate(enemyToSpawnData.enemyPrefab, spawnPoint.position, spawnPoint.rotation);

                IEnemy enemyScript = enemyInstance.GetComponent<IEnemy>();

                if (enemyScript != null)
                {
                    enemyScript.Initialize(enemyToSpawnData, roomManager);
                }
                else
                {
                    Debug.LogWarning(
                        $"Spawned enemy '{enemyToSpawnData.enemyName}' from prefab '{enemyToSpawnData.enemyPrefab.name}' but it doesn't implement IEnemy. Cannot initialize with data.",
                        enemyInstance);
                }

                enemiesSpawnedThisRoom++;
                currentRoomBudget -= enemyToSpawnData.spawnCost;
            }

            currentRoom.hasEnemiesSpawned = true;
        }
    }
}