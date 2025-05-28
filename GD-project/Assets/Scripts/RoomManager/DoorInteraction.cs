using Helper;
using UnityEngine;

namespace RoomManager
{
    public class DoorInteraction : MonoBehaviour
    {
        [SerializeField] public Vector3Int direction;

        private Transform playerTransform;
        private RoomManager roomManager;
        private const float interactionDistance = 5f;
        
        public bool playerMoved = true;

        private void Start()
        {
            playerTransform = GameObject.FindWithTag("Player")?.transform;
            roomManager = FindFirstObjectByType<RoomManager>();
            SetDirectionFromName();
        }

        private void Update()
        {
            if (playerTransform == null || roomManager == null) return;

            var distance = Vector3.Distance(playerTransform.position, transform.position);

            if (distance <= interactionDistance && Input.GetKeyDown(KeyCode.L))
            {
                TryMovePlayer();
            }
        }

        private void SetDirectionFromName()
        {
            var doorName = gameObject.name.ToLower();

            if (doorName.Contains("top"))
                direction = Vector3Int.forward;
            else if (doorName.Contains("bottom"))
                direction = Vector3Int.back;
            else if (doorName.Contains("left"))
                direction = Vector3Int.left;
            else if (doorName.Contains("right"))
                direction = Vector3Int.right;
            else
                Debug.LogWarning($"Door '{gameObject.name}' has unknown direction. Please check naming!");
        }

        private void TryMovePlayer()
        {
            if (playerTransform == null || roomManager == null) return;

            var nextRoomIndex = roomManager.CurrentRoomIndex + direction;
            var nextRoom = roomManager.GetRoomScriptAt(nextRoomIndex);

            if (nextRoom != null)
            {
                playerMoved = false;
                FadeManager.Instance.FadeOutIn(() =>
                {
                    roomManager.CurrentRoomIndex = nextRoom.RoomIndex;
                    roomManager.SpawnPlayerInRoom(nextRoom.RoomIndex, direction);
                    Debug.Log($"Player moved to Room at {nextRoom.RoomIndex}");
                    //Invoke(nameof(SetPlayerMoved), 5000);
                });
            }
            else
            {
                Debug.Log("No room in that direction!");
            }
        }

        private void SetPlayerMoved()
        {
            playerMoved = true;
        }
    }
}