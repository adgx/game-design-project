using System;
using Helper;
using UnityEngine;

namespace RoomManager
{
    public class DoorInteraction : MonoBehaviour
    {
        [Tooltip("World direction this door leads to (e.g., (0,0,1) for North/Top, (1,0,0) for East/Right)")]
        [SerializeField] public Vector3Int doorLeadsToDirection;

        private Transform playerTransform;
        private RoomManager roomManager;
        private const float InteractionDistance = 5f;
        
        private FadeManager fadeManagerInstance; 

        private void Start()
        {
            playerTransform = GameObject.FindWithTag("Player")?.transform;
            if (!playerTransform) Debug.LogError("DoorInteraction: Player not found. Tag your player 'Player'.", this);

            roomManager = FindFirstObjectByType<RoomManager>();
            if (!roomManager) Debug.LogError("DoorInteraction: RoomManager not found in scene.", this);
            
            fadeManagerInstance = FindFirstObjectByType<FadeManager>(); 
            
            if (!fadeManagerInstance) Debug.LogWarning("DoorInteraction: FadeManager not found. Transitions will be instant.", this);
            
            if (doorLeadsToDirection == Vector3Int.zero)
            {
                SetDirectionFromName();
            }
        }

        private void Update()
        {
            if (!playerTransform || !roomManager) return;
            
            var distance = Vector3.Distance(playerTransform.position, transform.position);

            if (distance <= InteractionDistance && Input.GetKeyDown(KeyCode.X))
            {
                TryMovePlayer();
            }
        }

        private void SetDirectionFromName()
        {
            var doorName = gameObject.name.ToLower();

            if (doorName.Contains("topdoor"))
                doorLeadsToDirection = Vector3Int.forward;
            else if (doorName.Contains("bottomdoor"))
                doorLeadsToDirection = Vector3Int.back;
            else if (doorName.Contains("leftdoor"))
                doorLeadsToDirection = Vector3Int.left;
            else if (doorName.Contains("rightdoor"))
                doorLeadsToDirection = Vector3Int.right;
            else
                Debug.LogWarning($"Door '{gameObject.name}': Unknown direction from name. Please set 'Door Leads To Direction' in Inspector or check naming!", this);
        }

        private void TryMovePlayer()
        {
            if (!roomManager) return;
            
            Vector3Int nextRoomIndex = roomManager.CurrentRoomIndex + doorLeadsToDirection;
            Room nextRoomScript = roomManager.GetRoomScriptAt(nextRoomIndex);

            if (nextRoomScript)
            {
                Debug.Log($"DoorInteraction: Attempting to move to room {nextRoomIndex} via door leading {doorLeadsToDirection}.");
                
                Action moveAction = () => {
                    roomManager.SpawnPlayerInRoom(nextRoomScript.RoomIndex, doorLeadsToDirection);
                    
                    roomManager.NotifyPlayerEnteredRoom(nextRoomScript.RoomIndex);
                    
                    Debug.Log($"Player moved to Room at {nextRoomScript.RoomIndex}");
                };

                if (fadeManagerInstance)
                {
                    fadeManagerInstance.FadeOutIn(moveAction);
                }
                else
                {
                    moveAction();
                }
            }
            else
            {
                Debug.LogWarning($"DoorInteraction: No room found at {nextRoomIndex} in direction {doorLeadsToDirection} from {roomManager.CurrentRoomIndex}.");
            }
        }
    }
}