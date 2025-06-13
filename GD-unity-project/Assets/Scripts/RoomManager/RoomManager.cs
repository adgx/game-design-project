using System;
using System.Collections.Generic;
using System.Linq;
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

        public static float GetCellWorldWidth() => CellWorldWidth;
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

        [Header("Player References")]
        [Tooltip("Reference to the player GameObject. Will be found by tag if not assigned.")]
        [SerializeField]
        private GameObject _currentPlayer;

        [SerializeField] private FadeManagerLoadingScreen fadeManagerLoadingScreen;

        /// <summary>Gets the reference to the player's GameObject.</summary>
        public GameObject CurrentPlayer => _currentPlayer;

        public event Action OnRunReady;
        public event Action<Vector3Int> PlayerEnteredNewRoom;

        public bool IsPlayerSpawned { get; private set; }
        public Vector3Int CurrentRoomIndex { get; private set; }
        public bool IsNavMeshBaked { get; private set; }

        [Header("Grid Dimensions")] [SerializeField]
        private int _gridSizeX = 14;

        private const int GridSizeY = 1; // Vertical grid size is fixed at 1
        [SerializeField] private int _gridSizeZ = 14;

        private Dictionary<RoomType, List<RoomData.RoomData>> _roomDataByType =
            new Dictionary<RoomType, List<RoomData.RoomData>>();

        private List<Room> _roomInstances = new List<Room>();
        private Queue<Vector3Int> _roomsToProcessQueue = new Queue<Vector3Int>();
        private int[,,] _roomGrid;
        private int _roomCount;
        private bool _isGenerationComplete;

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
            GenerateRooms();
        }

        private void Update()
        {
            if (_roomsToProcessQueue.Count > 0 && _roomCount < _maxRooms && !_isGenerationComplete)
            {
                Vector3Int currentRoomGridIndex = _roomsToProcessQueue.Dequeue();
                TryExpandFrom(currentRoomGridIndex);
            }
            else if (!_isGenerationComplete)
            {
                if (_roomCount < _minRooms || _roomCount > _maxRooms)
                {
                    RegenerateRooms();
                }
                else
                {
                    _isGenerationComplete = true;
                    NavMeshSurface nav = GetComponent<NavMeshSurface>();
                    if (nav != null)
                    {
                        nav.BuildNavMesh();
                    }

                    IsNavMeshBaked = true;

                    if (!IsPlayerSpawned)
                    {
                        SpawnPlayerInRoom(CurrentRoomIndex);
                        IsPlayerSpawned = true;
                        fadeManagerLoadingScreen.Hide();
                    }

                    OnRunReady?.Invoke();
                }
            }
        }

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
                if (roomData == null || roomData.roomPrefab == null)
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
            GenerateRooms();
        }

        private void GenerateRooms()
        {
            foreach (var roomInst in _roomInstances.Where(r => r != null && r.gameObject != null))
            {
                Destroy(roomInst.gameObject);
            }

            _roomInstances.Clear();
            _roomsToProcessQueue.Clear();
            _roomGrid = new int[_gridSizeX, GridSizeY, _gridSizeZ];
            _roomCount = 0;
            _isGenerationComplete = false;
            IsPlayerSpawned = false;
            fadeManagerLoadingScreen.Show();

            if (_roomDataByType.Count == 0 || _roomDataByType.Values.All(list => list.Count == 0))
                CacheRoomDataByType();

            if (_initialRoomData == null)
            {
                Debug.LogError("Cannot start: Initial Room Data is not assigned in the Inspector!", this);
                return;
            }

            Vector3Int startGridIndex = new Vector3Int(_gridSizeX / 2, 0, _gridSizeZ / 2);
            Room initialRoom = PlaceNewRoomAt(startGridIndex, _initialRoomData);

            if (initialRoom != null)
            {
                CurrentRoomIndex = startGridIndex;
            }
            else
            {
                Debug.LogError("Failed to place initial room!", this);
            }
        }

        private Room PlaceNewRoomAt(Vector3Int gridIndex, RoomData.RoomData roomData)
        {
            if (roomData == null || roomData.roomPrefab == null) return null;
            if (!IsInBounds(gridIndex) || _roomGrid[gridIndex.x, gridIndex.y, gridIndex.z] != 0) return null;

            Vector3 worldPosition = GetWorldPositionFromGridIndex(gridIndex);
            GameObject roomGO = Instantiate(roomData.roomPrefab, worldPosition, Quaternion.identity, this.transform);

            Room newRoomScript = roomGO.GetComponent<Room>();
            if (newRoomScript == null)
            {
                Debug.LogError($"Prefab '{roomData.roomPrefab.name}' is missing a Room component. Destroying instance.",
                    roomGO);
                Destroy(roomGO);
                return null;
            }

            newRoomScript.Initialize(roomData, this);
            newRoomScript.RoomIndex = gridIndex;

            _roomGrid[gridIndex.x, gridIndex.y, gridIndex.z] = _roomInstances.Count + 1;
            _roomInstances.Add(newRoomScript);
            _roomCount++;

            if (_roomCount < _maxRooms)
                _roomsToProcessQueue.Enqueue(gridIndex);

            return newRoomScript;
        }

        private void TryExpandFrom(Vector3Int currentRoomGridIndex)
        {
            Room currentRoomScript = GetRoomByGridIndex(currentRoomGridIndex);
            if (currentRoomScript == null) return;

            var shuffledConnectors = currentRoomScript.GetAvailableConnectors().OrderBy(c => Random.value);

            foreach (RoomConnector currentConnectorInfo in shuffledConnectors)
            {
                if (_roomCount >= _maxRooms) break;
                if (_roomCount > 1 && Random.value < _skipConnectionChance) continue;

                ConnectorDirection expansionDirection = currentConnectorInfo.Direction;
                Vector3Int neighborGridIndex = currentRoomGridIndex + GetVectorFromLocalDirection(expansionDirection);

                if (!IsInBounds(neighborGridIndex) ||
                    _roomGrid[neighborGridIndex.x, neighborGridIndex.y, neighborGridIndex.z] != 0) continue;

                ConnectorDirection requiredConnectorOnNewRoom = GetOppositeDirection(expansionDirection);

                List<RoomData.RoomData> suitableRoomData = new List<RoomData.RoomData>();

                foreach (var roomDataList in _roomDataByType.Values)
                {
                    foreach (var roomData in roomDataList)
                    {
                        if (roomData.roomPrefab.GetComponent<Room>().GetConnector(requiredConnectorOnNewRoom) != null)
                            suitableRoomData.Add(roomData);
                    }
                }

                if (suitableRoomData.Count == 0) continue;

                RoomData.RoomData dataToPlace = suitableRoomData[Random.Range(0, suitableRoomData.Count)];
                Room neighborRoomScript = PlaceNewRoomAt(neighborGridIndex, dataToPlace);

                if (neighborRoomScript != null)
                {
                    currentRoomScript.ActivateConnection(expansionDirection);
                    neighborRoomScript.ActivateConnection(requiredConnectorOnNewRoom);
                }
            }
        }

        /// <summary>
        /// Moves the player to a specific room and spawn point.
        /// </summary>
        /// <param name="roomIndex">The grid index of the room to spawn in.</param>
        /// <param name="worldEntryDirection">The world direction the player is coming from, to find the correct spawn point.</param>
        public void SpawnPlayerInRoom(Vector3Int roomIndex, Vector3Int? worldEntryDirection = null)
        {
            if (_currentPlayer == null) return;

            Room roomToSpawnIn = GetRoomByGridIndex(roomIndex);
            if (roomToSpawnIn == null) return;

            CurrentRoomIndex = roomIndex;
            Transform spawnPoint = worldEntryDirection.HasValue
                ? roomToSpawnIn.GetSpawnPointForWorldEntryDirection(worldEntryDirection.Value)
                : roomToSpawnIn.CentralSpawnPoint;

            _currentPlayer.transform.position =
                spawnPoint != null ? spawnPoint.position : roomToSpawnIn.transform.position;
        }

        /// <summary>
        /// Retrieves the Room component instance at a given grid index.
        /// </summary>
        /// <returns>The Room script instance, or null if no room exists there.</returns>
        public Room GetRoomByGridIndex(Vector3Int gridIndex)
        {
            return _roomInstances.FirstOrDefault(r => r != null && r.RoomIndex == gridIndex);
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
            if (_roomGrid == null || !IsInBounds(gridIndex)) return false;
            return _roomGrid[gridIndex.x, gridIndex.y, gridIndex.z] != 0;
        }

        /// <summary>
        /// Called by other scripts (like doors) to notify the manager of a room change.
        /// </summary>
        public void NotifyPlayerEnteredNewRoom(Vector3Int newRoomIndex)
        {
            CurrentRoomIndex = newRoomIndex;
            PlayerEnteredNewRoom?.Invoke(newRoomIndex);
        }
    }
}