using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviour
{
    /// <summary>
    /// The prefab to instantiate for each room.
    /// </summary>
    [SerializeField] private GameObject roomPrefab;

    /// <summary>
    /// The maximum number of rooms to generate.
    /// </summary>
    [SerializeField] private int maxRooms = 15;

    /// <summary>
    /// The minimum number of rooms to generate.
    /// </summary>
    [SerializeField] private int minRooms = 5;

    /// <summary>
    /// The reference to the current room
    /// </summary>
    [SerializeField] public Vector3Int currentRoom;
    
    /// <summary>
    /// The reference to the current player
    /// </summary>
    [SerializeField] public GameObject currentPlayer = null;

    /// <summary>
    /// The width of each room.
    /// </summary>
    private int roomWidth = 20;

    /// <summary>
    /// The height of each room.
    /// </summary>
    private int roomHeight = 10;

    /// <summary>
    /// The depth of each room.
    /// </summary>
    private int roomDepth = 20;

    /// <summary>
    /// The size of the grid in the X direction.
    /// </summary>
    private int gridSizeX = 10;

    /// <summary>
    /// The size of the grid in the Y direction (fixed to 1 for 2D).
    /// </summary>
    private int gridSizeY = 1;

    /// <summary>
    /// The size of the grid in the Z direction.
    /// </summary>
    private int gridSizeZ = 10;

    /// <summary>
    /// A list of all instantiated room objects.
    /// </summary>
    private List<GameObject> roomObjects = new List<GameObject>();

    /// <summary>
    /// A queue to manage room generation expansion.
    /// </summary>
    private Queue<Vector3Int> roomQueue = new Queue<Vector3Int>();

    /// <summary>
    /// A 3D array that tracks the grid of rooms.
    /// </summary>
    private int[,,] roomGrid;

    /// <summary>
    /// The current number of rooms that have been generated.
    /// </summary>
    private int roomCount;

    /// <summary>
    /// A flag indicating whether the room generation is complete.
    /// </summary>
    private bool generationComplete = false;

    /// <summary>
    /// Start is called before the first frame update.
    /// Initializes room generation.
    /// </summary>
    private void Start()
    {
        GenerateRooms();
    }

    /// <summary>
    /// Update is called once per frame.
    /// Handles the logic for room expansion and regeneration.
    /// </summary>
    private void Update()
    {
        if (roomQueue.Count > 0 && roomCount < maxRooms && !generationComplete)
        {
            // Dequeue and attempt to expand in all 4 cardinal directions.
            Vector3Int roomIndex = roomQueue.Dequeue();
            TryGenerateRoom(new Vector3Int(roomIndex.x - 1, roomIndex.y, roomIndex.z));
            TryGenerateRoom(new Vector3Int(roomIndex.x + 1, roomIndex.y, roomIndex.z));
            TryGenerateRoom(new Vector3Int(roomIndex.x, roomIndex.y, roomIndex.z - 1));
            TryGenerateRoom(new Vector3Int(roomIndex.x, roomIndex.y, roomIndex.z + 1));
        }
        else if (generationComplete && (roomCount < minRooms || roomCount > maxRooms))
        {
            // If room count is not within valid range, regenerate rooms.
            RegenerateRooms();
        }
        else if (!generationComplete)
        {
            // When generation is complete, log the result and mark it as done.
            Debug.Log("Generation completed with room count: " + roomCount);
            generationComplete = true;
        }
    }

    /// <summary>
    /// Initializes the room grid and starts the room generation process.
    /// </summary>
    private void GenerateRooms()
    {
        roomGrid = new int[gridSizeX, gridSizeY, gridSizeZ];
        roomQueue.Clear();
        roomObjects.Clear();
        roomCount = 0;
        generationComplete = false;

        // Start generating rooms from the center of the grid.
        Vector3Int startRoom = new Vector3Int(gridSizeX / 2, 0, gridSizeZ / 2);
        EnqueueInitialRoom(startRoom);
        currentRoom = startRoom;
    }

    /// <summary>
    /// Adds the initial room to the queue and instantiates it.
    /// </summary>
    /// <param name="roomIndex">The grid position of the room to instantiate.</param>
    private void EnqueueInitialRoom(Vector3Int roomIndex)
    {
        roomQueue.Enqueue(roomIndex);
        roomGrid[roomIndex.x, roomIndex.y, roomIndex.z] = 1; // Mark as occupied
        roomCount++; // Increase room count

        // Instantiate the room prefab at the calculated position.
        var room = Instantiate(roomPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
        room.name = $"Room-{roomCount}";
        var roomScript = room.GetComponent<Room>();
        roomScript.RoomIndex = roomIndex;
        roomObjects.Add(room); // Add the room to the list of room objects
    }

    /// <summary>
    /// Attempts to generate a room at the given grid index.
    /// </summary>
    /// <param name="index">The grid position to attempt room generation at.</param>
    /// <returns>True if the room was successfully generated, false otherwise.</returns>
    private bool TryGenerateRoom(Vector3Int index)
    {
        int x = index.x;
        int y = index.y;
        int z = index.z;

        // Check if the room position is valid (within bounds) and available
        if (!IsInBounds(x, y, z)) return false;
        if (roomGrid[x, y, z] != 0) return false; // Room already exists
        if (roomCount >= maxRooms) return false; // Max rooms reached
        if (Random.value < 0.5f) return false; // Random chance to limit room generation
        if (CountAdjacentRooms(index) > 1) return false; // Avoid overcrowding by checking adjacent rooms

        // Mark this position as occupied and add to the room count
        roomGrid[x, y, z] = 1;
        roomCount++;
        roomQueue.Enqueue(index); // Add to the queue for further expansion

        // Instantiate the new room at the calculated position.
        var newRoom = Instantiate(roomPrefab, GetPositionFromGridIndex(index), Quaternion.identity);
        newRoom.name = $"Room-{roomCount}";
        newRoom.GetComponent<Room>().RoomIndex = index;
        roomObjects.Add(newRoom); // Add the new room to the list of room objects

        // Open doors between adjacent rooms.
        OpenDoors(newRoom, x, y, z);

        return true;
    }

    /// <summary>
    /// Checks if a grid position is within the bounds of the room grid.
    /// </summary>
    /// <param name="x">The X-coordinate of the position to check.</param>
    /// <param name="y">The Y-coordinate of the position to check.</param>
    /// <param name="z">The Z-coordinate of the position to check.</param>
    /// <returns>True if the position is within bounds, false otherwise.</returns>
    private bool IsInBounds(int x, int y, int z)
    {
        return x >= 0 && x < gridSizeX &&
               y >= 0 && y < gridSizeY &&
               z >= 0 && z < gridSizeZ;
    }

    /// <summary>
    /// Regenerates the rooms when the current generation is invalid (either too few or too many rooms).
    /// </summary>
    private void RegenerateRooms()
    {
        foreach (var room in roomObjects)
        {
            if (room != null)
            {
                Destroy(room); // Destroy existing rooms
            }
        }

        GenerateRooms(); // Restart the room generation process
    }

    /// <summary>
    /// Counts the number of adjacent rooms surrounding the given position.
    /// </summary>
    /// <param name="index">The position to check for adjacent rooms.</param>
    /// <returns>The count of adjacent rooms.</returns>
    private int CountAdjacentRooms(Vector3Int index)
    {
        int x = index.x;
        int y = index.y;
        int z = index.z;

        int count = 0;

        // Check if adjacent cells have rooms and count them.
        if (IsInBounds(x - 1, y, z) && roomGrid[x - 1, y, z] != 0) count++;
        if (IsInBounds(x + 1, y, z) && roomGrid[x + 1, y, z] != 0) count++;
        if (IsInBounds(x, y, z - 1) && roomGrid[x, y, z - 1] != 0) count++;
        if (IsInBounds(x, y, z + 1) && roomGrid[x, y, z + 1] != 0) count++;

        return count;
    }

    /// <summary>
    /// Opens doors between adjacent rooms based on their positions.
    /// </summary>
    /// <param name="room">The newly instantiated room.</param>
    /// <param name="x">The X-coordinate of the room's position.</param>
    /// <param name="y">The Y-coordinate of the room's position.</param>
    /// <param name="z">The Z-coordinate of the room's position.</param>
    private void OpenDoors(GameObject room, int x, int y, int z)
    {
        // Get the script component of the new room to interact with its door system.
        Room newRoomScript = room.GetComponent<Room>();

        // Check and open doors to adjacent rooms based on grid position.
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

    /// <summary>
    /// Retrieves the <see cref="Room"/> script attached to the room at the given grid index.
    /// </summary>
    /// <param name="index">The grid position of the room to retrieve.</param>
    /// <returns>The <see cref="Room"/> script, or null if the room is not found.</returns>
    private Room GetRoomScriptAt(Vector3Int index)
    {
        GameObject roomObject = roomObjects.Find(r => r.GetComponent<Room>().RoomIndex == index);
        return roomObject?.GetComponent<Room>();
    }

    /// <summary>
    /// Converts a grid index into a world-space position for room instantiation.
    /// </summary>
    /// <param name="gridIndex">The grid index to convert.</param>
    /// <returns>The corresponding world position.</returns>
    private Vector3 GetPositionFromGridIndex(Vector3Int gridIndex)
    {
        return new Vector3(
            roomWidth * (gridIndex.x - gridSizeX / 2),
            0, // Fixed height, since it's 2D generation
            roomDepth * (gridIndex.z - gridSizeZ / 2)
        );
    }

    /// <summary>
    /// Draws the room grid in the Unity editor for debugging purposes.
    /// </summary>
    private void OnDrawGizmos()
    {
        Color gizmoColor = new Color(0, 1, 1, 0.05f);
        Gizmos.color = gizmoColor;

        // Loop through the entire grid and draw a wireframe box at each position.
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Vector3 position = GetPositionFromGridIndex(new Vector3Int(x, y, z));
                    Gizmos.DrawWireCube(position, new Vector3(roomWidth, roomHeight, roomDepth));
                }
            }
        }
    }
}
