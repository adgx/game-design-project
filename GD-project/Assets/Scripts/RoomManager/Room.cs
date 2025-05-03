using UnityEngine;

public class Room : MonoBehaviour
{
    /// <summary>
    /// The door object for the top side of the room.
    /// </summary>
    [SerializeField] private GameObject topDoor;

    /// <summary>
    /// The door object for the bottom side of the room.
    /// </summary>
    [SerializeField] private GameObject bottomDoor;

    /// <summary>
    /// The door object for the right side of the room.
    /// </summary>
    [SerializeField] private GameObject rightDoor;

    /// <summary>
    /// The door object for the left side of the room.
    /// </summary>
    [SerializeField] private GameObject leftDoor;

    /// <summary>
    /// The index (position) of the room in the grid.
    /// </summary>
    public Vector3Int RoomIndex { get; set; }

    /// <summary>
    /// Opens the door in the specified direction.
    /// </summary>
    /// <param name="direction">The direction to open the door (forward, back, left, or right).</param>
    public void OpenDoor(Vector3Int direction)
    {
        if (direction == Vector3Int.forward && topDoor != null)
        {
            topDoor.SetActive(true); // Open the top door
        }
        else if (direction == Vector3Int.back && bottomDoor != null)
        {
            bottomDoor.SetActive(true); // Open the bottom door
        }
        else if (direction == Vector3Int.right && rightDoor != null)
        {
            rightDoor.SetActive(true); // Open the right door
        }
        else if (direction == Vector3Int.left && leftDoor != null)
        {
            leftDoor.SetActive(true); // Open the left door
        }
    }

    /// <summary>
    /// Closes all doors in the room.
    /// </summary>
    public void CloseAllDoors()
    {
        if (topDoor != null) topDoor.SetActive(false);
        if (bottomDoor != null) bottomDoor.SetActive(false);
        if (rightDoor != null) rightDoor.SetActive(false);
        if (leftDoor != null) leftDoor.SetActive(false);
    }
}