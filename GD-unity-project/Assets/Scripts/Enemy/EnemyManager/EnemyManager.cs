using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoomManager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy.EnemyManager
{
    /// <summary>
    /// Manages the spawning of enemies within rooms based on budget and spawn points.
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }

        [Header("Spawning Configuration")]
        [Tooltip("A list of all possible enemy types that can be spawned in the dungeon.")]
        [SerializeField]
        private List<EnemyData.EnemyData> _availableEnemyData;

        [Tooltip("The minimum number of enemies that must be spawned in a room if possible.")] [SerializeField]
        private int _minEnemiesPerRoom = 1;

        [Tooltip("The maximum number of enemies that can be spawned in a single room in Loop 1.")] [SerializeField]
        private int _maxEnemiesPerRoomLoop1 = 3;

		[Tooltip("The maximum number of enemies that can be spawned in a single room.")]
		[SerializeField]
		private int _maxEnemiesPerRoom = 3;

		private RoomManager.RoomManager _roomManager;

		// List with all the indexes of the rooms where enemies have already spawned
		private List<Vector3Int> roomsEnemiesSpawnedIndexes = new List<Vector3Int>();

        private List<GameObject> spawnedEnemies = new List<GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (_availableEnemyData == null || _availableEnemyData.Count == 0)
            {
                Debug.LogError("EnemyManager: 'Available Enemy Data' list is empty! Cannot spawn enemies.", this);
                enabled = false;
            }
        }

        private void Start()
        {


            _roomManager = RoomManager.RoomManager.Instance;

            if (_roomManager != null)
            {
                _roomManager.PlayerEnteredNewRoom += HandlePlayerEnteredNewRoom;
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
            if (_roomManager != null)
            {
                _roomManager.PlayerEnteredNewRoom -= HandlePlayerEnteredNewRoom;
            }
        }

        private void HandlePlayerEnteredNewRoom(Vector3Int newRoomIndex)
        {
            if (_roomManager == null) return;

            Room currentRoom = _roomManager.GetRoomByGridIndex(newRoomIndex);

            if (currentRoom == null || currentRoom.HasEnemiesSpawned || roomsEnemiesSpawnedIndexes.Contains(newRoomIndex) || currentRoom.EnemySpawnPoints.Count == 0) return;

            int remainingBudget = currentRoom.MaxSpawnCost;
            int enemiesSpawnedCount = 0;

            List<Transform> shuffledSpawnPoints = currentRoom.EnemySpawnPoints
                .Where(sp => sp != null)
                .OrderBy(sp => Random.value)
                .ToList();

			for (int i = 0; i < _maxEnemiesPerRoom && shuffledSpawnPoints.Count > 0 && remainingBudget > 0; i++)
            {
                List<EnemyData.EnemyData> affordableEnemies = _availableEnemyData
                    .Where(enemyData => enemyData != null && enemyData.enemyPrefab != null && enemyData.spawnCost > 0 &&
                                        enemyData.spawnCost <= remainingBudget)
                    .OrderBy(enemyData => enemyData.spawnCost)
                    .ToList();

				if (affordableEnemies.Count == 0) break;

                EnemyData.EnemyData enemyToSpawnData = (enemiesSpawnedCount < _minEnemiesPerRoom)
                    ? affordableEnemies.FirstOrDefault()
                    : affordableEnemies[Random.Range(0, affordableEnemies.Count)];

                if (enemyToSpawnData == null) continue;

                Transform spawnPoint = shuffledSpawnPoints[0];
                shuffledSpawnPoints.RemoveAt(0);

                GameObject enemyInstance =
                    Instantiate(enemyToSpawnData.enemyPrefab, spawnPoint.position, spawnPoint.rotation);

                if(enemyInstance.GetComponent<IEnemy>() is { } enemyScript) {
                    enemyScript.Initialize(enemyToSpawnData, _roomManager);
                    spawnedEnemies.Add(enemyInstance);
                }
                else {
                    Debug.LogWarning(
                        $"Spawned enemy '{enemyToSpawnData.enemyName}' from prefab '{enemyToSpawnData.enemyPrefab.name}' but it doesn't implement IEnemy.",
                        enemyInstance);
                }

                enemiesSpawnedCount++;
                remainingBudget -= enemyToSpawnData.spawnCost;
            }

            currentRoom.HasEnemiesSpawned = true;

			if(!roomsEnemiesSpawnedIndexes.Contains(newRoomIndex)) {
				roomsEnemiesSpawnedIndexes.Add(newRoomIndex);
			}
		}

        public void SetEnemyDifficulty() {
			foreach(EnemyData.EnemyData enemyData in _availableEnemyData) {
				enemyData.setDifficulty();
			}

            _maxEnemiesPerRoom = _maxEnemiesPerRoomLoop1 + ((int)GameStatus.loopIteration > 0 ? 1 : 0);
		}

        public void DestroyAllEnemies() {
            foreach(GameObject enemy in spawnedEnemies) {
                Destroy(enemy);
            }

            spawnedEnemies.Clear();
			roomsEnemiesSpawnedIndexes = new List<Vector3Int>();
		}
    }
}