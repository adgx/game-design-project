using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RoomManager
{
    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }

        private const float CellWorldWidth = 130f;
        private const float CellWorldDepth = 130f;

        public static float GetCellWorldWidth() => CellWorldWidth;
        public static float GetCellWorldDepth() => CellWorldDepth;

        [Header("Room Generation Settings")] [SerializeField]
        private List<GameObject> allRoomPrefabs;

        [SerializeField] private int maxRooms = 15;
        [SerializeField] private int minRooms = 10;
        [Range(0f, 1f)] [SerializeField] private float skipConnectionChance = 0.35f;

        [Header("Player References")] public GameObject currentPlayer;

        public event Action OnRunReady;
        public event Action<Vector3Int> PlayerEnteredNewRoom;

        public bool playerHasSpawned { get; private set; }
        public Vector3Int CurrentRoomIndex { get; private set; }

        private const int GridSizeX = 10;
        private const int GridSizeY = 1;
        private const int GridSizeZ = 10;

        private Dictionary<RoomType, List<GameObject>> sortedRoomPrefabs = new Dictionary<RoomType, List<GameObject>>();
        private List<Room> roomInstances = new List<Room>();
        private Queue<Vector3Int> roomsToProcessQueue = new Queue<Vector3Int>();
        private int[,,] roomGrid;
        private int roomCount;
        private bool generationComplete;
        public bool navMashBaked = false;

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

            SortPrefabs();
            if (currentPlayer == null)
            {
                currentPlayer = GameObject.FindWithTag("Player");
                if (currentPlayer == null)
                    Debug.LogError("PLAYER NOT FOUND. Tag player or assign in Inspector.");
            }
        }

        private void Start()
        {
            GenerateRooms();
        }

        private void Update()
        {
            if (roomsToProcessQueue.Count > 0 && roomCount < maxRooms && !generationComplete)
            {
                Vector3Int currentRoomGridIndex = roomsToProcessQueue.Dequeue();
                TryExpandFrom(currentRoomGridIndex);
            }
            else if (!generationComplete)
            {
                if (roomCount < minRooms || roomCount > maxRooms)
                {
                    RegenerateRooms();
                }
                else
                {
                    generationComplete = true;
                    NavMeshSurface nav = GetComponent<NavMeshSurface>();
                    nav.BuildNavMesh();
                    navMashBaked = true;

                    if (!playerHasSpawned)
                    {
                        SpawnPlayerInRoom(CurrentRoomIndex);
                        playerHasSpawned = true;
                    }

                    OnRunReady?.Invoke();
                }
            }
        }

        private void SortPrefabs()
        {
            sortedRoomPrefabs.Clear();
            foreach (RoomType rt in Enum.GetValues(typeof(RoomType)))
            {
                sortedRoomPrefabs.Add(rt, new List<GameObject>());
            }

            if (allRoomPrefabs == null || allRoomPrefabs.Count == 0)
            {
                Debug.LogError("RoomManager: 'All Room Prefabs' list is empty!");
                enabled = false;
                return;
            }

            foreach (GameObject prefab in allRoomPrefabs)
            {
                if (prefab == null)
                    continue;
                Room roomComponent = prefab.GetComponent<Room>();
                if (roomComponent != null)
                {
                    sortedRoomPrefabs[roomComponent.roomType].Add(prefab);
                }
                else
                {
                    Debug.LogWarning($"Prefab '{prefab.name}' missing Room component.");
                }
            }
        }

        public void RegenerateRooms()
        {
            GenerateRooms();
        }

        private void GenerateRooms()
        {
            foreach (var roomInst in roomInstances.Where(r => r != null && r.gameObject != null))
            {
                Destroy(roomInst.gameObject);
            }

            roomInstances.Clear();
            roomsToProcessQueue.Clear();
            roomGrid = new int[GridSizeX, GridSizeY, GridSizeZ];
            roomCount = 0;
            generationComplete = false;
            playerHasSpawned = false;

            if (sortedRoomPrefabs.Count == 0 || sortedRoomPrefabs.Values.All(list => list.Count == 0))
                SortPrefabs();

            List<GameObject> initialRoomCandidates = GetPrefabsOfType(RoomType.Room);
            if (initialRoomCandidates.Count == 0)
            {
                Debug.LogError("Cannot start: No prefabs of type 'Room' found!");
                return;
            }

            Vector3Int startGridIndex = new Vector3Int(GridSizeX / 2, 0, GridSizeZ / 2);
            GameObject startPrefab = initialRoomCandidates[Random.Range(0, initialRoomCandidates.Count)];
            Room initialRoom = PlaceNewRoomAt(startGridIndex, startPrefab);
            if (initialRoom != null)
            {
                CurrentRoomIndex = startGridIndex;
            }
            else
            {
                Debug.LogError("Failed to place initial room!");
            }
        }

        private List<GameObject> GetPrefabsOfType(RoomType type)
        {
            if (sortedRoomPrefabs.TryGetValue(type, out List<GameObject> prefabs) && prefabs != null &&
                prefabs.Count > 0)
            {
                return prefabs;
            }

            return new List<GameObject>();
        }

        private Room PlaceNewRoomAt(Vector3Int gridIndex, GameObject prefabToUse)
        {
            if (prefabToUse == null)
                return null;
            if (!IsInBounds(gridIndex) || roomGrid[gridIndex.x, gridIndex.y, gridIndex.z] != 0)
                return null;

            Vector3 worldPosition = GetWorldPositionFromGridIndex(gridIndex);
            GameObject roomGO = Instantiate(prefabToUse, worldPosition, Quaternion.identity);
            Room newRoomScript = roomGO.GetComponent<Room>();

            if (newRoomScript == null)
            {
                Destroy(roomGO);
                return null;
            }

            newRoomScript.RoomIndex = gridIndex;
            roomGrid[gridIndex.x, gridIndex.y, gridIndex.z] = roomInstances.Count + 1;
            roomInstances.Add(newRoomScript);
            roomCount++;
            if (roomCount < maxRooms)
                roomsToProcessQueue.Enqueue(gridIndex);
            return newRoomScript;
        }

        private void TryExpandFrom(Vector3Int currentRoomGridIndex)
        {
            Room currentRoomScript = GetRoomByGridIndex(currentRoomGridIndex);
            if (currentRoomScript == null)
                return;

            var availableConnectors = currentRoomScript.GetAvailableConnectors();
            if (availableConnectors.Count == 0)
                return;

            var shuffledConnectors = availableConnectors.OrderBy(c => Random.value);

            foreach (RoomConnector currentConnectorInfo in shuffledConnectors)
            {
                if (roomCount >= maxRooms)
                    break;

                if (roomCount > 1 && Random.value < skipConnectionChance)
                {
                    continue;
                }

                ConnectorDirection expansionDirection = currentConnectorInfo.direction;
                Vector3Int neighborGridIndex = currentRoomGridIndex + GetVectorFromLocalDirection(expansionDirection);

                if (!IsInBounds(neighborGridIndex) ||
                    roomGrid[neighborGridIndex.x, neighborGridIndex.y, neighborGridIndex.z] != 0)
                {
                    continue;
                }

                ConnectorDirection requiredConnectorOnNewRoom = GetOppositeDirection(expansionDirection);

                List<GameObject> suitablePrefabs = new List<GameObject>();
                foreach (List<GameObject> prefabListByType in sortedRoomPrefabs.Values)
                {
                    foreach (GameObject prefab in prefabListByType)
                    {
                        Room roomData = prefab.GetComponent<Room>();
                        if (roomData != null && roomData.GetConnector(requiredConnectorOnNewRoom) != null)
                        {
                            if (roomData.roomType == RoomType.Room)
                            {
                                suitablePrefabs.Add(prefab);
                            }
                            else if (expansionDirection == ConnectorDirection.North ||
                                     expansionDirection == ConnectorDirection.South)
                            {
                                if (roomData.roomType == RoomType.Corridor_NS)
                                {
                                    suitablePrefabs.Add(prefab);
                                }
                            }
                            else
                            {
                                if (roomData.roomType == RoomType.Corridor_EW)
                                {
                                    suitablePrefabs.Add(prefab);
                                }
                            }
                        }
                    }
                }

                if (suitablePrefabs.Count == 0)
                {
                    continue;
                }

                GameObject prefabToPlace = suitablePrefabs[Random.Range(0, suitablePrefabs.Count)];

                Room neighborRoomScript = PlaceNewRoomAt(neighborGridIndex, prefabToPlace);
                if (neighborRoomScript != null)
                {
                    currentRoomScript.ActivateConnection(expansionDirection);
                    neighborRoomScript.ActivateConnection(requiredConnectorOnNewRoom);
                }
            }
        }

        public void SpawnPlayerInRoom(Vector3Int roomIndex, Vector3Int? worldEntryDirection = null)
        {
            if (currentPlayer == null)
                return;
            Room roomToSpawnIn = GetRoomByGridIndex(roomIndex);
            if (roomToSpawnIn == null)
                return;
            CurrentRoomIndex = roomIndex;
            Transform spawnPoint = worldEntryDirection.HasValue
                ? roomToSpawnIn.GetSpawnPointForWorldEntryDirection(worldEntryDirection.Value)
                : roomToSpawnIn.centralSpawnPoint;
            currentPlayer.transform.position =
                spawnPoint != null ? spawnPoint.position : roomToSpawnIn.transform.position;
        }

        public Room GetRoomByGridIndex(Vector3Int gridIndex)
        {
            return roomInstances.FirstOrDefault(r => r != null && r.RoomIndex == gridIndex);
        }

        private Vector3 GetWorldPositionFromGridIndex(Vector3Int gridIndex)
        {
            float xPos = (gridIndex.x - GridSizeX / 2f + 0.5f) * CellWorldWidth;
            float zPos = (gridIndex.z - GridSizeZ / 2f + 0.5f) * CellWorldDepth;
            return new Vector3(xPos, 0, zPos);
        }

        private bool IsInBounds(Vector3Int gridIndex)
        {
            return gridIndex.x >= 0 && gridIndex.x < GridSizeX &&
                   gridIndex.y >= 0 && gridIndex.y < GridSizeY &&
                   gridIndex.z >= 0 && gridIndex.z < GridSizeZ;
        }

        public static ConnectorDirection GetOppositeDirection(ConnectorDirection dir)
        {
            switch (dir)
            {
                case ConnectorDirection.North:
                    return ConnectorDirection.South;
                case ConnectorDirection.South:
                    return ConnectorDirection.North;
                case ConnectorDirection.East:
                    return ConnectorDirection.West;
                case ConnectorDirection.West:
                    return ConnectorDirection.East;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), "Invalid ConnectorDirection.");
            }
        }

        public static Vector3Int GetVectorFromLocalDirection(ConnectorDirection dir)
        {
            switch (dir)
            {
                case ConnectorDirection.North:
                    return Vector3Int.forward;
                case ConnectorDirection.South:
                    return Vector3Int.back;
                case ConnectorDirection.East:
                    return Vector3Int.right;
                case ConnectorDirection.West:
                    return Vector3Int.left;
                default:
                    return Vector3Int.zero;
            }
        }

        public static ConnectorDirection GetOppositeLocalDirection(Vector3Int worldGridOffsetPlayerCameFrom)
        {
            if (worldGridOffsetPlayerCameFrom == Vector3Int.forward)
                return ConnectorDirection.South;
            if (worldGridOffsetPlayerCameFrom == Vector3Int.back)
                return ConnectorDirection.North;
            if (worldGridOffsetPlayerCameFrom == Vector3Int.right)
                return ConnectorDirection.West;
            if (worldGridOffsetPlayerCameFrom == Vector3Int.left)
                return ConnectorDirection.East;
            return ConnectorDirection.South;
        }

        public static Vector3 GetWorldVectorFromLocalDirection(ConnectorDirection dir, Transform roomTransform)
        {
            switch (dir)
            {
                case ConnectorDirection.North:
                    return roomTransform.forward;
                case ConnectorDirection.South:
                    return -roomTransform.forward;
                case ConnectorDirection.East:
                    return roomTransform.right;
                case ConnectorDirection.West:
                    return -roomTransform.right;
                default:
                    return Vector3.zero;
            }
        }

        public int GetGridSizeX() => GridSizeX;
        public int GetGridSizeZ() => GridSizeZ;

        public bool DoesRoomExistAt(Vector3Int gridIndex)
        {
            if (roomGrid == null || !IsInBounds(gridIndex))
                return false;
            return roomGrid[gridIndex.x, gridIndex.y, gridIndex.z] != 0;
        }

        public void NotifyPlayerEnteredNewRoom(Vector3Int newRoomIndex)
        {
            PlayerEnteredNewRoom?.Invoke(newRoomIndex);
        }
    }
}