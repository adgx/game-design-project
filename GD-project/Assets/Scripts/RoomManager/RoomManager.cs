using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RoomManager
{
    public class RoomManager : MonoBehaviour
    {
        [SerializeField] private GameObject roomPrefab;
        [SerializeField] private int maxRooms = 15;
        [SerializeField] private int minRooms = 5;
        
        public event Action OnRunReady;

        public GameObject currentPlayer;
        public bool playerHasSpawned;

        public Vector3Int CurrentRoomIndex { get; set; }

        private const int RoomWidth = 20 * 5;
        private const int RoomDepth = 20 * 5;

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
            currentPlayer = GameObject.FindWithTag("Player");
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
                TryGenerateRoom(new Vector3Int(roomIndex.x - 1, roomIndex.y, roomIndex.z));
                TryGenerateRoom(new Vector3Int(roomIndex.x + 1, roomIndex.y, roomIndex.z));
                TryGenerateRoom(new Vector3Int(roomIndex.x, roomIndex.y, roomIndex.z - 1));
                TryGenerateRoom(new Vector3Int(roomIndex.x, roomIndex.y, roomIndex.z + 1));
            }
            else
                switch (generationComplete)
                {
                    case true when (roomCount < minRooms || roomCount > maxRooms):
                        RegenerateRooms();
                        break;
                    case false:
                    {
                        Debug.Log("Generation completed with room count: " + roomCount);
                        generationComplete = true;

                        if (!playerHasSpawned)
                        {
                            SpawnPlayerInRoom(CurrentRoomIndex);
                            playerHasSpawned = true;
                            
                            OnRunReady?.Invoke();
                        }

                        break;
                    }
                }
        }

        public void SpawnPlayerInRoom(Vector3Int roomIndex, Vector3Int? entryDirection = null)
        {
            var roomObject = roomObjects.FirstOrDefault(room => room.GetComponent<Room>().RoomIndex == roomIndex);
            if (!roomObject) return;
            

            var roomScript = roomObject.GetComponent<Room>();
            if (!roomScript || !currentPlayer) return;

            if (entryDirection == Vector3Int.forward)
            {
                currentPlayer.transform.position = roomScript.bottomSpawnPoint.position;
            }
            else if (entryDirection == Vector3Int.back)
            {
                currentPlayer.transform.position = roomScript.topSpawnPoint.position;
            }
            else if (entryDirection == Vector3Int.left)
            {
                currentPlayer.transform.position = roomScript.rightSpawnPoint.position;
            }
            else if (entryDirection == Vector3Int.right)
            {
                currentPlayer.transform.position = roomScript.leftSpawnPoint.position;
            }
            else
            {
                currentPlayer.transform.position = roomScript.centralSpawnPoint.position;
            }
        }

        private void GenerateRooms()
        {
            roomGrid = new int[GridSizeX, GridSizeY, GridSizeZ];
            roomQueue.Clear();
            roomObjects.Clear();
            roomCount = 0;
            generationComplete = false;

            var startRoomIndex = new Vector3Int(GridSizeX / 2, 0, GridSizeZ / 2);
            EnqueueInitialRoom(startRoomIndex);
            CurrentRoomIndex = startRoomIndex;
        }

        private void EnqueueInitialRoom(Vector3Int roomIndex)
        {
            roomQueue.Enqueue(roomIndex);
            roomGrid[roomIndex.x, roomIndex.y, roomIndex.z] = 1;
            roomCount++;

            var room = Instantiate(roomPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
            room.name = $"Room-{roomCount}";
            var roomScript = room.GetComponent<Room>();
            roomScript.RoomIndex = roomIndex;
            roomObjects.Add(room);
        }

        private void TryGenerateRoom(Vector3Int index)
        {
            var x = index.x;
            var y = index.y;
            var z = index.z;

            if (!IsInBounds(x, y, z)) return;
            if (roomGrid[x, y, z] != 0) return;
            if (roomCount >= maxRooms) return;
            if (Random.value < 0.5f) return;
            if (CountAdjacentRooms(index) > 1) return;

            roomGrid[x, y, z] = 1;
            roomCount++;
            roomQueue.Enqueue(index);

            var newRoom = Instantiate(roomPrefab, GetPositionFromGridIndex(index), Quaternion.identity);
            newRoom.name = $"Room-{roomCount}";
            newRoom.GetComponent<Room>().RoomIndex = index;
            roomObjects.Add(newRoom);

            OpenDoors(newRoom, x, y, z);
        }

        private static bool IsInBounds(int x, int y, int z)
        {
            return x is >= 0 and < GridSizeX &&
                   y is >= 0 and < GridSizeY &&
                   z is >= 0 and < GridSizeZ;
        }

        public void RegenerateRooms()
        {
            foreach (var room in roomObjects.Where(room => room != null))
            {
                Destroy(room);
            }

            playerHasSpawned = false;
            GenerateRooms();
        }

        private int CountAdjacentRooms(Vector3Int index)
        {
            int x = index.x;
            int y = index.y;
            int z = index.z;

            int count = 0;

            if (IsInBounds(x - 1, y, z) && roomGrid[x - 1, y, z] != 0) count++;
            if (IsInBounds(x + 1, y, z) && roomGrid[x + 1, y, z] != 0) count++;
            if (IsInBounds(x, y, z - 1) && roomGrid[x, y, z - 1] != 0) count++;
            if (IsInBounds(x, y, z + 1) && roomGrid[x, y, z + 1] != 0) count++;

            return count;
        }

        private void OpenDoors(GameObject room, int x, int y, int z)
        {
            var newRoomScript = room.GetComponent<Room>();

            if (IsInBounds(x - 1, y, z) && roomGrid[x - 1, y, z] != 0)
            {
                var adj = GetRoomScriptAt(new Vector3Int(x - 1, y, z));
                newRoomScript?.OpenDoor(Vector3Int.left);
                adj?.OpenDoor(Vector3Int.right);
                newRoomScript.Doors |= Room.DoorFlags.Left;
                adj.Doors |= Room.DoorFlags.Right;
            }

            if (IsInBounds(x + 1, y, z) && roomGrid[x + 1, y, z] != 0)
            {
                var adj = GetRoomScriptAt(new Vector3Int(x + 1, y, z));
                newRoomScript?.OpenDoor(Vector3Int.right);
                adj?.OpenDoor(Vector3Int.left);
                newRoomScript.Doors |= Room.DoorFlags.Right;
                adj.Doors |= Room.DoorFlags.Left;
            }

            if (IsInBounds(x, y, z - 1) && roomGrid[x, y, z - 1] != 0)
            {
                var adj = GetRoomScriptAt(new Vector3Int(x, y, z - 1));
                newRoomScript?.OpenDoor(Vector3Int.back);
                adj?.OpenDoor(Vector3Int.forward);
                newRoomScript.Doors |= Room.DoorFlags.Bottom;
                adj.Doors |= Room.DoorFlags.Top;
            }

            if (IsInBounds(x, y, z + 1) && roomGrid[x, y, z + 1] != 0)
            {
                var adj = GetRoomScriptAt(new Vector3Int(x, y, z + 1));
                newRoomScript?.OpenDoor(Vector3Int.forward);
                adj?.OpenDoor(Vector3Int.back);
                newRoomScript.Doors |= Room.DoorFlags.Top;
                adj.Doors |= Room.DoorFlags.Bottom;
            }
        }

        public Room GetRoomScriptAt(Vector3Int index)
        {
            var roomObject = roomObjects.Find(r => r.GetComponent<Room>().RoomIndex == index);
            return roomObject?.GetComponent<Room>();
        }

        private static Vector3 GetPositionFromGridIndex(Vector3Int gridIndex)
        {
            return new Vector3(
                RoomWidth * (gridIndex.x - GridSizeX / 2),
                0,
                RoomDepth * (gridIndex.z - GridSizeZ / 2)
            );
        }
    }
}