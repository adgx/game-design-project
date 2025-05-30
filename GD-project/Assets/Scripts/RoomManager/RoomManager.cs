using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RoomManager
{
    public class RoomManager : MonoBehaviour
    {
        [Header("Room Generation Settings")] [SerializeField]
        private GameObject roomPrefab;

        [SerializeField] private int maxRooms = 15;
        [SerializeField] private int minRooms = 5;

        [Header("Player References")] public GameObject currentPlayer;

        public event Action OnRunReady;
        public event Action<Vector3Int> PlayerEnteredNewRoom;

        public bool PlayerHasSpawned { get; private set; }
        public Vector3Int CurrentRoomIndex { get; private set; }

        // Room and Grid Dimensions (World Space)
        private const int RoomWidth = 20 * 5;
        private const int RoomDepth = 20 * 5;

        // Grid Dimensions (Logical Grid)
        private const int GridSizeX = 10;
        private const int GridSizeY = 1;
        private const int GridSizeZ = 10;

        private List<GameObject> roomObjects = new List<GameObject>();
        private Queue<Vector3Int> roomQueue = new Queue<Vector3Int>();
        private int[,,] roomGrid;
        private int roomCount;
        private bool generationComplete;

        private void Awake()
        {
            if (currentPlayer) return;
            currentPlayer = GameObject.FindWithTag("Player");
            if (!currentPlayer)
            {
                Debug.LogError("RoomManager: Player GameObject not found! Assign it or tag it correctly.");
            }
        }

        private void Start()
        {
            GenerateRooms();
        }

        private void Update()
        {
            if (roomQueue.Count > 0 && roomCount < maxRooms && !generationComplete)
            {
                var roomIndex = roomQueue.Dequeue();
                TryGenerateRoom(new Vector3Int(roomIndex.x - 1, roomIndex.y, roomIndex.z)); // West
                TryGenerateRoom(new Vector3Int(roomIndex.x + 1, roomIndex.y, roomIndex.z)); // East
                TryGenerateRoom(new Vector3Int(roomIndex.x, roomIndex.y, roomIndex.z - 1)); // South/Back
                TryGenerateRoom(new Vector3Int(roomIndex.x, roomIndex.y, roomIndex.z + 1)); // North/Forward
            }
            else
            {
                if (generationComplete)
                {
                    return;
                }

                if (roomCount < minRooms || roomCount > maxRooms)
                {
                    Debug.LogWarning(
                        $"RoomManager: Generation finished but constraints violated. Rooms: {roomCount} (Min: {minRooms}, Max: {maxRooms}). Regenerating...");
                    RegenerateRooms();
                }
                else
                {
                    Debug.Log("RoomManager: Generation completed successfully with room count: " + roomCount);
                    generationComplete = true;

                    if (!PlayerHasSpawned)
                    {
                        SpawnPlayerInRoom(CurrentRoomIndex);
                        PlayerHasSpawned = true;
                    }

                    Debug.Log(
                        $"RoomManager: Run ready. Player spawned: {PlayerHasSpawned}, Current Room: {CurrentRoomIndex}");
                    OnRunReady?.Invoke(); // Notify minimap and others that the new valid map is ready.
                }
            }
        }

        public void RegenerateRooms()
        {
            Debug.Log("RoomManager: RegenerateRooms() called.");
            GenerateRooms();
        }

        private void GenerateRooms()
        {
            Debug.Log("RoomManager: GenerateRooms() starting.");
            // 1. Clean up old rooms
            foreach (var roomGo in roomObjects.Where(r => r))
            {
                Destroy(roomGo);
            }

            roomObjects.Clear();

            // 2. Reset state variables
            roomGrid = new int[GridSizeX, GridSizeY, GridSizeZ];
            roomQueue.Clear();
            roomCount = 0;
            generationComplete = false; // Reset for the new generation attempt
            PlayerHasSpawned = false; // Player needs to be "re-spawned" in the new map

            // 3. Define and enqueue the starting room
            var startRoomIndex = new Vector3Int(GridSizeX / 2, 0, GridSizeZ / 2);
            CurrentRoomIndex = startRoomIndex; // Set the designated start room for this new map
            EnqueueInitialRoom(startRoomIndex);
            Debug.Log(
                $"RoomManager: New generation started. Initial room enqueued at {startRoomIndex}. CurrentRoomIndex set to {CurrentRoomIndex}.");
        }

        private void EnqueueInitialRoom(Vector3Int roomIndex)
        {
            roomQueue.Enqueue(roomIndex);
            roomGrid[roomIndex.x, roomIndex.y, roomIndex.z] = 1; // Mark as room
            roomCount++;

            var roomGo = Instantiate(roomPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
            roomGo.name = $"Room-Start({roomIndex.x},{roomIndex.y},{roomIndex.z})";
            var roomScript = roomGo.GetComponent<Room>();
            if (roomScript)
            {
                roomScript.RoomIndex = roomIndex;
            }
            else
            {
                Debug.LogError($"Room prefab is missing 'Room' script component on {roomGo.name}");
            }

            roomObjects.Add(roomGo);
        }

        private void TryGenerateRoom(Vector3Int index)
        {
            if (!IsInBounds(index.x, index.y, index.z)) return;
            if (roomGrid[index.x, index.y, index.z] != 0) return; // Already a room or processed
            if (roomCount >= maxRooms) return; // Max rooms limit reached
            if (Random.value < 0.5f) return; // Random chance to not build
            if (CountAdjacentRooms(index) > 1) return; // Avoid overly connected rooms early on

            roomGrid[index.x, index.y, index.z] = 1; // Mark as room
            roomCount++;
            roomQueue.Enqueue(index);

            var newRoomGo = Instantiate(roomPrefab, GetPositionFromGridIndex(index), Quaternion.identity);
            newRoomGo.name = $"Room-{roomCount}({index.x},{index.y},{index.z})";
            var roomScript = newRoomGo.GetComponent<Room>();
            if (roomScript)
            {
                roomScript.RoomIndex = index;
            }
            else
            {
                Debug.LogError($"Room prefab is missing 'Room' script component on {newRoomGo.name}");
            }

            roomObjects.Add(newRoomGo);

            OpenDoors(newRoomGo, index.x, index.y, index.z);
        }

        public void SpawnPlayerInRoom(Vector3Int roomIndex, Vector3Int? entryDirection = null)
        {
            if (!currentPlayer)
            {
                Debug.LogError("RoomManager: Cannot spawn player, currentPlayer is null.");
                return;
            }

            var roomObject = roomObjects.FirstOrDefault(room => room.GetComponent<Room>().RoomIndex == roomIndex);
            if (!roomObject)
            {
                Debug.LogError($"RoomManager: Cannot find room object at index {roomIndex} to spawn player.");
                return;
            }

            var roomScript = roomObject.GetComponent<Room>();
            if (!roomScript)
            {
                Debug.LogError($"RoomManager: Room object at {roomIndex} is missing Room script component.");
                return;
            }

            CurrentRoomIndex = roomIndex;

            Vector3 spawnPosition = roomScript.centralSpawnPoint
                ? roomScript.centralSpawnPoint.position
                : roomObject.transform.position;

            if (entryDirection.HasValue)
            {
                if (entryDirection.Value == Vector3Int.forward && roomScript.bottomSpawnPoint)
                    spawnPosition = roomScript.bottomSpawnPoint.position;
                else if (entryDirection.Value == Vector3Int.back && roomScript.topSpawnPoint)
                    spawnPosition = roomScript.topSpawnPoint.position;
                else if (entryDirection.Value == Vector3Int.left && roomScript.rightSpawnPoint)
                    spawnPosition = roomScript.rightSpawnPoint.position;
                else if (entryDirection.Value == Vector3Int.right && roomScript.leftSpawnPoint)
                    spawnPosition = roomScript.leftSpawnPoint.position;
            }

            currentPlayer.transform.position = spawnPosition;
            Debug.Log(
                $"RoomManager: Player spawned in room {roomIndex} at {spawnPosition}. Entry direction: {entryDirection?.ToString() ?? "None"}.");
        }

        private void OpenDoors(GameObject room, int x, int y, int z)
        {
            var newRoomScript = room.GetComponent<Room>();
            if (!newRoomScript) return;
            
            if (IsInBounds(x - 1, y, z) && roomGrid[x - 1, y, z] != 0)
            {
                var adj = GetRoomScriptAt(new Vector3Int(x - 1, y, z));
                newRoomScript.OpenDoor(Vector3Int.left);
                adj?.OpenDoor(Vector3Int.right);
                newRoomScript.Doors |= Room.DoorFlags.Left;
                if (adj) adj.Doors |= Room.DoorFlags.Right;
            }
            
            if (IsInBounds(x + 1, y, z) && roomGrid[x + 1, y, z] != 0)
            {
                var adj = GetRoomScriptAt(new Vector3Int(x + 1, y, z));
                newRoomScript.OpenDoor(Vector3Int.right);
                adj?.OpenDoor(Vector3Int.left);
                newRoomScript.Doors |= Room.DoorFlags.Right;
                if (adj) adj.Doors |= Room.DoorFlags.Left;
            }
            
            if (IsInBounds(x, y, z - 1) && roomGrid[x, y, z - 1] != 0)
            {
                var adj = GetRoomScriptAt(new Vector3Int(x, y, z - 1));
                newRoomScript.OpenDoor(Vector3Int.back);
                adj?.OpenDoor(Vector3Int.forward);
                newRoomScript.Doors |= Room.DoorFlags.Bottom;
                if (adj) adj.Doors |= Room.DoorFlags.Top;
            }
            
            if (IsInBounds(x, y, z + 1) && roomGrid[x, y, z + 1] != 0)
            {
                var adj = GetRoomScriptAt(new Vector3Int(x, y, z + 1));
                newRoomScript.OpenDoor(Vector3Int.forward);
                adj?.OpenDoor(Vector3Int.back);
                newRoomScript.Doors |= Room.DoorFlags.Top;
                if (adj) adj.Doors |= Room.DoorFlags.Bottom;
            }
        }

        public Room GetRoomScriptAt(Vector3Int index)
        {
            var roomObject = roomObjects.Find(r => r && r.GetComponent<Room>()?.RoomIndex == index);
            return roomObject?.GetComponent<Room>();
        }

        private Vector3 GetPositionFromGridIndex(Vector3Int gridIndex)
        {
            return new Vector3(
                RoomWidth * (gridIndex.x - GridSizeX / 2f),
                0,
                RoomDepth * (gridIndex.z - GridSizeZ / 2f)
            );
        }

        private bool IsInBounds(int x, int y, int z)
        {
            return x is >= 0 and < GridSizeX &&
                   y is >= 0 and < GridSizeY &&
                   z is >= 0 and < GridSizeZ;
        }

        private int CountAdjacentRooms(Vector3Int index)
        {
            int count = 0;
            if (IsInBounds(index.x - 1, index.y, index.z) && roomGrid[index.x - 1, index.y, index.z] != 0) count++;
            if (IsInBounds(index.x + 1, index.y, index.z) && roomGrid[index.x + 1, index.y, index.z] != 0) count++;
            if (IsInBounds(index.x, index.y, index.z - 1) && roomGrid[index.x, index.y, index.z - 1] != 0) count++;
            if (IsInBounds(index.x, index.y, index.z + 1) && roomGrid[index.x, index.y, index.z + 1] != 0) count++;
            return count;
        }
        
        public static int GetGridSizeX() => GridSizeX;
        public static int GetGridSizeZ() => GridSizeZ;

        public bool DoesRoomExistAt(Vector3Int gridIndex)
        {
            if (roomGrid == null) return false;
            if (IsInBounds(gridIndex.x, gridIndex.y, gridIndex.z))
            {
                return roomGrid[gridIndex.x, gridIndex.y, gridIndex.z] == 1;
            }

            return false;
        }

        public void NotifyPlayerEnteredRoom(Vector3Int roomIndex)
        {
            PlayerEnteredNewRoom?.Invoke(roomIndex);
        }
    }
}