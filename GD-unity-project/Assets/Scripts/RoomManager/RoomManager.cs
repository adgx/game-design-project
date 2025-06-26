using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enemy.EnemyData;
using RoomManager.RoomData;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RoomManager
{
    /// <summary>
    /// Manages the procedural generation, layout, and state of all rooms in a dungeon level.
    /// </summary>
    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }

        private const float CellWorldWidth = 200f;
        private const float CellWorldDepth = 200f;

        /// <summary>
        /// Gets the world width of a single grid cell.
        /// </summary>
        public static float GetCellWorldWidth() => CellWorldWidth;

        /// <summary>
        /// Gets the world depth of a single grid cell.
        /// </summary>
        public static float GetCellWorldDepth() => CellWorldDepth;

        [Header("Room Generation Settings")]
        [Tooltip("A list of all possible RoomData assets that can be used for random generation.")]
        [SerializeField]
        private List<RoomData.RoomData> _availableRooms;

        [Tooltip("The specific RoomData asset to be used for the very first room.")] [SerializeField]
        private RoomData.RoomData _initialRoomData;

        [Tooltip("The maximum number of rooms to generate in the dungeon.")] [SerializeField]
        private int _maxRooms = 15;

        [Tooltip("The minimum number of rooms required. The dungeon will regenerate if it has fewer.")] [SerializeField]
        private int _minRooms = 10;

        [Tooltip("The chance (0 to 1) to skip creating a connection, leading to more dead ends.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float _skipConnectionChance = 0.35f;

        [Tooltip("The maximum number of vending machines that could spawn in the dungeon.")] [SerializeField]
        private int _maxVendingMachines = 4;

        [Tooltip("The maximum number of upgrade terminal that could spawn in the dungeon.")] [SerializeField]
        private int _maxUpgradeTerminal = 4;

        [Header("Player References")]
        [Tooltip("Reference to the player GameObject. Will be found by tag if not assigned.")]
        [SerializeField]
        private GameObject _currentPlayer;

        [SerializeField] private FadeManagerLoadingScreen fadeManagerLoadingScreen;

        /// <summary>
        /// Gets the reference to the player's GameObject.
        /// </summary>
        public GameObject CurrentPlayer => _currentPlayer;

        /// <summary>
        /// Event triggered when the run is ready to begin.
        /// </summary>
        public event Action OnRunReady;

        /// <summary>
        /// Event triggered when the player enters a new room.
        /// </summary>
        public event Action<Vector3Int> PlayerEnteredNewRoom;

        /// <summary>
        /// Event triggered when a room is fully instantiated.
        /// </summary>
        public event Action OnRoomFullyInstantiated;

        public bool IsPlayerSpawned { get; private set; }
        public Vector3Int CurrentRoomIndex { get; private set; }
        public bool IsNavMeshBaked { get; private set; }

        [Header("Grid Dimensions")] [SerializeField]
        private int _gridSizeX = 14;

        private const int GridSizeY = 1;
        [SerializeField] private int _gridSizeZ = 14;

        private Dictionary<RoomType, List<RoomData.RoomData>> _roomDataByType =
            new Dictionary<RoomType, List<RoomData.RoomData>>();

        private RoomData.RoomData[,,] _roomGridData;
        private Room _currentRoomInstance;

        private bool _isLayoutGenerated;
        private Queue<Vector3Int> _roomsToProcessQueue = new Queue<Vector3Int>();
        private int[,,] _roomGrid;
        private int _roomCount;

        private int _vendingMachineSpawned = 0;
        private int _upgradeTerminalSpawned = 0;
        private bool _paperAlreadySpawned = false;

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

            CacheRoomDataByType();

            if (_currentPlayer == null)
            {
                _currentPlayer = GameObject.FindWithTag("Player");

                if (_currentPlayer == null)
                    Debug.LogError("PLAYER NOT FOUND. Tag player or assign in Inspector.");
            }
        }

        private void Start()
        {
            GenerateLayout();
        }

        /// <summary>
        /// Caches available rooms grouped by their room type.
        /// </summary>
        private void CacheRoomDataByType()
        {
            _roomDataByType.Clear();

            foreach (RoomType rt in Enum.GetValues(typeof(RoomType)))
            {
                _roomDataByType.Add(rt, new List<RoomData.RoomData>());
            }

            if (_availableRooms == null || _availableRooms.Count == 0)
            {
                Debug.LogError("RoomManager: 'Available Rooms' list is empty!", this);
                enabled = false;
                return;
            }

            foreach (RoomData.RoomData roomData in _availableRooms)
            {
                if (!roomData || !roomData.roomPrefab)
                {
                    Debug.LogWarning(
                        "Found a null entry or a RoomData with a missing prefab in 'Available Rooms'. Skipping.", this);
                    continue;
                }

                _roomDataByType[roomData.roomType].Add(roomData);
            }
        }

        /// <summary>
        /// Destroys the current dungeon and generates a new one from scratch.
        /// </summary>
        public void RegenerateRooms()
        {
            GenerateLayout();
        }

        /// <summary>
        /// Generates the layout of the dungeon procedurally.
        /// </summary>
        private void GenerateLayout()
        {
            _roomGridData = new RoomData.RoomData[_gridSizeX, GridSizeY, _gridSizeZ];
			_roomsToProcessQueue.Clear();
            UnloadCurrentRoom();
            _roomCount = 0;
            _isLayoutGenerated = false;
            IsPlayerSpawned = false;
            _upgradeTerminalSpawned = 0;
            _vendingMachineSpawned = 0;

            if (GameStatus.gameStarted)
                fadeManagerLoadingScreen.Show();

            if (_roomDataByType.Count == 0 || _roomDataByType.Values.All(list => list.Count == 0))
            {
                CacheRoomDataByType();
            }

            if (!_initialRoomData)
            {
                Debug.LogError("Cannot start: Initial Room Data is not assigned in the Inspector!", this);
                return;
            }

            Vector3Int startGridIndex = new Vector3Int(_gridSizeX / 2, 0, _gridSizeZ / 2);
            _roomGridData[startGridIndex.x, startGridIndex.y, startGridIndex.z] = _initialRoomData;
            _roomCount++;
            _roomsToProcessQueue.Enqueue(startGridIndex);

            while (_roomsToProcessQueue.Count > 0 && _roomCount < _maxRooms)
            {
                Vector3Int currentRoomGridIndex = _roomsToProcessQueue.Dequeue();
                TryExpandFrom(currentRoomGridIndex);
            }

            if (_roomCount < _minRooms)
            {
                RegenerateRooms();
                return;
            }

            _isLayoutGenerated = true;
            CurrentRoomIndex = startGridIndex;
            Room initialRoom = LoadRoomAt(CurrentRoomIndex);
            SpawnPlayerInRoom(initialRoom);
            IsPlayerSpawned = true;
            fadeManagerLoadingScreen.Hide();

            OnRunReady?.Invoke();
        }

        /// <summary>
        /// Waits until end of frame before notifying that the room is ready.
        /// </summary>
        private IEnumerator NotifyRoomReady()
        {
            yield return new WaitForEndOfFrame();
            OnRoomFullyInstantiated?.Invoke();
        }

        /// <summary>
        /// Instantiates and initializes the room at the specified grid index.
        /// </summary>
        private Room LoadRoomAt(Vector3Int gridIndex)
        {
            RoomData.RoomData roomDataToLoad = _roomGridData[gridIndex.x, gridIndex.y, gridIndex.z];

            if (!roomDataToLoad)
            {
                Debug.LogError($"No RoomData found in layout at {gridIndex} to load.", this);
                return null;
            }

            Vector3 worldPosition = GetWorldPositionFromGridIndex(gridIndex);
            GameObject roomGameObject = Instantiate(roomDataToLoad.roomPrefab, worldPosition, Quaternion.identity,
                this.transform);
            Room newRoomScript = roomGameObject.GetComponent<Room>();

			newRoomScript.Initialize(roomDataToLoad, this);
            newRoomScript.RoomIndex = gridIndex;
            newRoomScript.PostInitializeConnections();

            bool isTerminalSpawned = false;

            if (_upgradeTerminalSpawned < _maxUpgradeTerminal)
            {
                bool isSpawned = newRoomScript.PostInitializeUpgradeTerminal();

                if (isSpawned) _upgradeTerminalSpawned++;

                isTerminalSpawned = isSpawned;
            }

            if (!isTerminalSpawned && _vendingMachineSpawned < _maxVendingMachines)
            {
                bool isSpawed = newRoomScript.PostInitializeVendingMachine();

                if (isSpawed) _vendingMachineSpawned++;
            }

            if (!_paperAlreadySpawned && newRoomScript.PostInitializePaper())
            {
                _paperAlreadySpawned = true;
            }

            _currentRoomInstance = newRoomScript;
            
            NavMeshSurface nav = GetComponent<NavMeshSurface>();
            // The navmesh mantains the current and the previous rooms navmeshes
            nav.BuildNavMesh();
            IsNavMeshBaked = true;
            
            return newRoomScript;
        }

        /// <summary>
        /// Destroys the currently loaded room.
        /// </summary>
        private void UnloadCurrentRoom()
        {
            if (_currentRoomInstance == null) return;

            Destroy(_currentRoomInstance.gameObject);
            _currentRoomInstance = null;
        }

        /// <summary>
        /// Attempts to expand the dungeon layout from the specified room grid index.
        /// </summary>
        private void TryExpandFrom(Vector3Int currentRoomGridIndex)
        {
            RoomData.RoomData currentRoomData =
                _roomGridData[currentRoomGridIndex.x, currentRoomGridIndex.y, currentRoomGridIndex.z];
            Room currentRoomPrefabScript = currentRoomData.roomPrefab.GetComponent<Room>();

            if (!currentRoomPrefabScript) return;

            var shuffledConnectors = currentRoomPrefabScript.GetAvailableConnectors().OrderBy(c => Random.value);

            foreach (RoomConnector currentConnectorInfo in shuffledConnectors)
            {
                if (_roomCount >= _maxRooms) break;
                if (_roomCount > 1 && Random.value < _skipConnectionChance) continue;

                ConnectorDirection expansionDirection = currentConnectorInfo.Direction;
                Vector3Int neighborGridIndex = currentRoomGridIndex + GetVectorFromLocalDirection(expansionDirection);

                if (!IsInBounds(neighborGridIndex) ||
                    _roomGridData[neighborGridIndex.x, neighborGridIndex.y, neighborGridIndex.z]) continue;

                ConnectorDirection requiredConnectorOnNewRoom = GetOppositeDirection(expansionDirection);

                List<RoomData.RoomData> suitableRoomData = FindSuitableRoomData(requiredConnectorOnNewRoom);

                if (suitableRoomData.Count == 0) continue;

                RoomData.RoomData dataToPlace = suitableRoomData[Random.Range(0, suitableRoomData.Count)];

                _roomGridData[neighborGridIndex.x, neighborGridIndex.y, neighborGridIndex.z] = dataToPlace;
                _roomCount++;
                _roomsToProcessQueue.Enqueue(neighborGridIndex);
            }
        }

        /// <summary>
        /// Finds a list of RoomData assets that have the specified required connector direction.
        /// </summary>
        private List<RoomData.RoomData> FindSuitableRoomData(ConnectorDirection requiredConnector)
        {
            var suitableRooms = new List<RoomData.RoomData>();

            foreach (var roomDataList in _roomDataByType.Values)
            {
                foreach (var roomData in roomDataList)
                {
                    if (!roomData || !roomData.roomPrefab) continue;

                    Room roomComponent = roomData.roomPrefab.GetComponent<Room>();
                    if (!roomComponent) continue;

                    if (roomComponent.GetConnector(requiredConnector) != null)
                    {
                        suitableRooms.Add(roomData);
                    }
                }
            }

            return suitableRooms;
        }

        /// <summary>
        /// Loads a new room and spawns the player inside it.
        /// </summary>
        public void TraverseRoom(Vector3Int newRoomIndex, Vector3Int entryDirection)
        {
            if (!_isLayoutGenerated || !DoesRoomExistAt(newRoomIndex))
            {
                Debug.LogError($"Attempted to traverse to a non-existent room at {newRoomIndex}", this);
                return;
            }

            UnloadCurrentRoom();
            Room newRoom = LoadRoomAt(newRoomIndex);
            StartCoroutine(NotifyRoomReady());
            SpawnPlayerInRoom(newRoom, entryDirection);
            NotifyPlayerEnteredNewRoom(newRoomIndex);
        }

        /// <summary>
        /// Moves the player to a specific room and spawn point.
        /// </summary>
        public void SpawnPlayerInRoom(Room roomToSpawnIn, Vector3Int? worldEntryDirection = null)
        {
            if (!_currentPlayer || !roomToSpawnIn) return;

            Transform spawnPoint = worldEntryDirection.HasValue
                ? roomToSpawnIn.GetSpawnPointForWorldEntryDirection(worldEntryDirection.Value)
                : roomToSpawnIn.CentralSpawnPoint;

            _currentPlayer.transform.position = spawnPoint ? spawnPoint.position : roomToSpawnIn.transform.position;
        }

        /// <summary>
        /// Retrieves the Room component instance at a given grid index.
        /// </summary>
        public Room GetRoomByGridIndex(Vector3Int gridIndex)
        {
            if (_currentRoomInstance != null && _currentRoomInstance.RoomIndex == gridIndex)
            {
                return _currentRoomInstance;
            }

            return null;
        }

        private Vector3 GetWorldPositionFromGridIndex(Vector3Int gridIndex)
        {
            float xPos = (gridIndex.x - _gridSizeX / 2f + 0.5f) * CellWorldWidth;
            float zPos = (gridIndex.z - _gridSizeZ / 2f + 0.5f) * CellWorldDepth;
            return new Vector3(xPos, 0, zPos);
        }

        private bool IsInBounds(Vector3Int gridIndex)
        {
            return gridIndex.x >= 0 && gridIndex.x < _gridSizeX &&
                   gridIndex.y >= 0 && gridIndex.y < GridSizeY &&
                   gridIndex.z >= 0 && gridIndex.z < _gridSizeZ;
        }

        public static ConnectorDirection GetOppositeDirection(ConnectorDirection dir)
        {
            switch (dir)
            {
                case ConnectorDirection.North: return ConnectorDirection.South;
                case ConnectorDirection.South: return ConnectorDirection.North;
                case ConnectorDirection.East: return ConnectorDirection.West;
                case ConnectorDirection.West: return ConnectorDirection.East;
                default: throw new ArgumentOutOfRangeException(nameof(dir), "Invalid ConnectorDirection.");
            }
        }

        public static Vector3Int GetVectorFromLocalDirection(ConnectorDirection dir)
        {
            switch (dir)
            {
                case ConnectorDirection.North: return Vector3Int.forward;
                case ConnectorDirection.South: return Vector3Int.back;
                case ConnectorDirection.East: return Vector3Int.right;
                case ConnectorDirection.West: return Vector3Int.left;
                default: return Vector3Int.zero;
            }
        }

        public static ConnectorDirection GetOppositeLocalDirection(Vector3Int worldGridOffsetPlayerCameFrom)
        {
            if (worldGridOffsetPlayerCameFrom == Vector3Int.forward) return ConnectorDirection.South;
            if (worldGridOffsetPlayerCameFrom == Vector3Int.back) return ConnectorDirection.North;
            if (worldGridOffsetPlayerCameFrom == Vector3Int.right) return ConnectorDirection.West;
            if (worldGridOffsetPlayerCameFrom == Vector3Int.left) return ConnectorDirection.East;
            return ConnectorDirection.South;
        }

        public static Vector3 GetWorldVectorFromLocalDirection(ConnectorDirection dir, Transform roomTransform)
        {
            switch (dir)
            {
                case ConnectorDirection.North: return roomTransform.forward;
                case ConnectorDirection.South: return -roomTransform.forward;
                case ConnectorDirection.East: return roomTransform.right;
                case ConnectorDirection.West: return -roomTransform.right;
                default: return Vector3.zero;
            }
        }

        public int GetGridSizeX() => _gridSizeX;
        public int GetGridSizeZ() => _gridSizeZ;

        /// <summary>
        /// Checks if a room has been generated at a specific grid index.
        /// </summary>
        public bool DoesRoomExistAt(Vector3Int gridIndex)
        {
            if (_roomGridData == null || !IsInBounds(gridIndex)) return false;
            return _roomGridData[gridIndex.x, gridIndex.y, gridIndex.z] != null;
        }

        /// <summary>
        /// Called by other scripts (like doors) to notify the manager of a room change.
        /// </summary>
        public void NotifyPlayerEnteredNewRoom(Vector3Int newRoomIndex)
        {
            CurrentRoomIndex = newRoomIndex;
            PlayerEnteredNewRoom?.Invoke(newRoomIndex);
        }

        public void SetRoomsDifficulty() {
			foreach(RoomData.RoomData roomData in _availableRooms) {
				roomData.setDifficulty();
			}
		}
    }
}