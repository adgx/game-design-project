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

        public GameObject currentPlayer;
        public bool playerHasSpawned;

        public Vector3Int CurrentRoomIndex { get; set; }

        private int roomWidth = 20 * 5;
        private int roomHeight = 10;
        private int roomDepth = 20 * 5;

        private int gridSizeX = 10;
        private int gridSizeY = 1;
        private int gridSizeZ = 10;

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
                        }

                        break;
                    }
                }
        }

        public void SpawnPlayerInRoom(Vector3Int roomIndex, Vector3Int? entryDirection = null)
        {
            var roomObject = roomObjects.FirstOrDefault(room => room.GetComponent<Room>().RoomIndex == roomIndex);
            if (roomObject == null) return;
            

            var roomScript = roomObject.GetComponent<Room>();
            if (roomScript == null || currentPlayer == null) return;

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
            roomGrid = new int[gridSizeX, gridSizeY, gridSizeZ];
            roomQueue.Clear();
            roomObjects.Clear();
            roomCount = 0;
            generationComplete = false;

            var startRoomIndex = new Vector3Int(gridSizeX / 2, 0, gridSizeZ / 2);
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

        private bool TryGenerateRoom(Vector3Int index)
        {
            var x = index.x;
            var y = index.y;
            var z = index.z;

            if (!IsInBounds(x, y, z)) return false;
            if (roomGrid[x, y, z] != 0) return false;
            if (roomCount >= maxRooms) return false;
            if (Random.value < 0.5f) return false;
            if (CountAdjacentRooms(index) > 1) return false;

            roomGrid[x, y, z] = 1;
            roomCount++;
            roomQueue.Enqueue(index);

            var newRoom = Instantiate(roomPrefab, GetPositionFromGridIndex(index), Quaternion.identity);
            newRoom.name = $"Room-{roomCount}";
            newRoom.GetComponent<Room>().RoomIndex = index;
            roomObjects.Add(newRoom);

            OpenDoors(newRoom, x, y, z);

            return true;
        }

        private bool IsInBounds(int x, int y, int z)
        {
            return x >= 0 && x < gridSizeX &&
                   y >= 0 && y < gridSizeY &&
                   z >= 0 && z < gridSizeZ;
        }

        private void RegenerateRooms()
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
            Room newRoomScript = room.GetComponent<Room>();

            if (IsInBounds(x - 1, y, z) && roomGrid[x - 1, y, z] != 0)
            {
                Room adj = GetRoomScriptAt(new Vector3Int(x - 1, y, z));
                newRoomScript?.OpenDoor(Vector3Int.left);
                adj?.OpenDoor(Vector3Int.right);
            }

            if (IsInBounds(x + 1, y, z) && roomGrid[x + 1, y, z] != 0)
            {
                Room adj = GetRoomScriptAt(new Vector3Int(x + 1, y, z));
                newRoomScript?.OpenDoor(Vector3Int.right);
                adj?.OpenDoor(Vector3Int.left);
            }

            if (IsInBounds(x, y, z - 1) && roomGrid[x, y, z - 1] != 0)
            {
                Room adj = GetRoomScriptAt(new Vector3Int(x, y, z - 1));
                newRoomScript?.OpenDoor(Vector3Int.back);
                adj?.OpenDoor(Vector3Int.forward);
            }

            if (IsInBounds(x, y, z + 1) && roomGrid[x, y, z + 1] != 0)
            {
                Room adj = GetRoomScriptAt(new Vector3Int(x, y, z + 1));
                newRoomScript?.OpenDoor(Vector3Int.forward);
                adj?.OpenDoor(Vector3Int.back);
            }
        }

        public Room GetRoomScriptAt(Vector3Int index)
        {
            GameObject roomObject = roomObjects.Find(r => r.GetComponent<Room>().RoomIndex == index);
            return roomObject?.GetComponent<Room>();
        }

        private Vector3 GetPositionFromGridIndex(Vector3Int gridIndex)
        {
            return new Vector3(
                roomWidth * (gridIndex.x - gridSizeX / 2),
                0,
                roomDepth * (gridIndex.z - gridSizeZ / 2)
            );
        }
    }
}