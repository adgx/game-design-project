using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    [SerializeField] public Vector3Int direction;

    private Transform player;
    private RoomManager roomManager;

    private void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        roomManager = FindAnyObjectByType<RoomManager>();
    }

    private void SetDirectionFromName()
    {
        var doorName = gameObject.name.ToLower();

        if (doorName.Contains("top"))
        {
            direction = Vector3Int.forward;
        }
        else if (doorName.Contains("bottom"))
        {
            direction = Vector3Int.back;
        }
        else if (doorName.Contains("left"))
        {
            direction = Vector3Int.left;
        }
        else if (doorName.Contains("right"))
        {
            direction = Vector3Int.right;
        }
        else
        {
            Debug.LogWarning($"Door '{gameObject.name}' has unknown direction. Please check naming!");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TryMovePlayer();
        }
    }

    private void TryMovePlayer()
    {
        if (player == null || currentRoom == null) return;

        Vector3Int nextRoomIndex = currentRoom.RoomIndex + direction;

        RoomManager manager = FindObjectOfType<RoomManager>();
        if (manager == null) return;

        Room nextRoom = manager.GetRoomScriptAt(nextRoomIndex);

        if (nextRoom != null)
        {
            Vector3 spawnPosition = GetSpawnPositionNearOppositeDoor(nextRoom);
            player.position = new Vector3(spawnPosition.x, player.position.y, spawnPosition.z); // Keep player's Y

            manager.CurrentRoom = nextRoom;
            Debug.Log($"Player moved to Room at {nextRoom.RoomIndex}");
        }
        else
        {
            Debug.Log("No room in that direction!");
        }
    }

    private Vector3 GetSpawnPositionNearOppositeDoor(Room nextRoom)
    {
        // Find opposite direction
        Vector3Int oppositeDirection = -direction;

        // Choose the correct door based on opposite direction
        Transform doorTransform = null;

        if (oppositeDirection == Vector3Int.forward && nextRoom.transform.Find("TopDoor"))
        {
            doorTransform = nextRoom.transform.Find("TopDoor");
        }
        else if (oppositeDirection == Vector3Int.back && nextRoom.transform.Find("BottomDoor"))
        {
            doorTransform = nextRoom.transform.Find("BottomDoor");
        }
        else if (oppositeDirection == Vector3Int.left && nextRoom.transform.Find("LeftDoor"))
        {
            doorTransform = nextRoom.transform.Find("LeftDoor");
        }
        else if (oppositeDirection == Vector3Int.right && nextRoom.transform.Find("RightDoor"))
        {
            doorTransform = nextRoom.transform.Find("RightDoor");
        }

        if (doorTransform != null)
        {
            // Slightly offset player from the door so they're not stuck inside it
            return doorTransform.position + doorTransform.forward * 1.5f;
        }

        // Fallback: center of the room
        return nextRoom.transform.position;
    }
}