// DoorInteraction.cs
using UnityEngine;

namespace RoomManager // Or your game's specific namespace
{
    public class DoorInteraction : MonoBehaviour
    {
        [Tooltip("The WORLD direction this exit leads to (e.g., North exit = (0,0,1)). Set in Inspector, or will try to infer from name.")]
        public Vector3Int leadsToWorldDirection; // This is the key direction for this door's function

        private RoomManager roomManager;
        private Room parentRoom; // The Room script this door trigger belongs to

        private const float INTERACTION_DISTANCE = 5f; // How close player needs to be
        public KeyCode interactionKey = KeyCode.E; // Key to press for interaction

        void Start()
        {
            roomManager = RoomManager.Instance; // Use the singleton instance from RoomManager
            parentRoom = GetComponentInParent<Room>(); // Assumes this script is on a child of a Room GameObject

            if (roomManager == null) Debug.LogError("DoorInteraction: RoomManager.Instance not found!", this);
            if (parentRoom == null) Debug.LogError("DoorInteraction: Parent Room component not found! Ensure this script is a child of a Room GameObject.", this);
            
            // If leadsToWorldDirection is not set in Inspector, try to guess it from GameObject name
            if (leadsToWorldDirection == Vector3Int.zero && parentRoom != null)
            {
                 InferDirectionFromName(); // Renamed for clarity
            }
        }
        
        // Tries to infer the door's leading direction based on its GameObject's name.
        private void InferDirectionFromName()
        {
            string myNameLower = gameObject.name.ToLower();
            if (myNameLower.Contains("north") || myNameLower.Contains("top")) leadsToWorldDirection = Vector3Int.forward;
            else if (myNameLower.Contains("south") || myNameLower.Contains("bottom")) leadsToWorldDirection = Vector3Int.back;
            else if (myNameLower.Contains("east") || myNameLower.Contains("right")) leadsToWorldDirection = Vector3Int.right;
            else if (myNameLower.Contains("west") || myNameLower.Contains("left")) leadsToWorldDirection = Vector3Int.left;
            else Debug.LogWarning($"DoorInteraction on '{gameObject.name}': Could not infer 'Leads To World Direction' from name. Please set it manually in Inspector for reliable behavior.", this);
        }

        void Update()
        {
            // Basic validity checks
            if (roomManager == null || parentRoom == null || roomManager.currentPlayer == null || !roomManager.playerHasSpawned) return;
            if (leadsToWorldDirection == Vector3Int.zero) return; // This door isn't configured to lead anywhere

            // Determine which local connector of the parent room this door trigger represents.
            // If this door leads North (world), it IS the North connector of its parent room.
            ConnectorDirection thisDoorsLocalConnectorDirection;
            if (leadsToWorldDirection == Vector3Int.forward) thisDoorsLocalConnectorDirection = ConnectorDirection.North;
            else if (leadsToWorldDirection == Vector3Int.back) thisDoorsLocalConnectorDirection = ConnectorDirection.South;
            else if (leadsToWorldDirection == Vector3Int.right) thisDoorsLocalConnectorDirection = ConnectorDirection.East;
            else if (leadsToWorldDirection == Vector3Int.left) thisDoorsLocalConnectorDirection = ConnectorDirection.West;
            else { 
                Debug.LogError($"DoorInteraction on {gameObject.name} in room {parentRoom.RoomIndex}: Invalid 'leadsToWorldDirection' ({leadsToWorldDirection}). Cannot determine local connector.", this);
                return; // Invalid configuration
            }

            RoomConnector connector = parentRoom.GetConnector(thisDoorsLocalConnectorDirection);

            // Check if the door is conceptually "open" and ready for interaction
            // It must be logically connected AND its passage visual must be active (if it has one)
            if (connector == null || !connector.isConnected || (connector.passageVisual != null && !connector.passageVisual.activeSelf))
            {
                // If not interactable, you might want to hide an interaction prompt UI here
                return; // Door is not "open" or doesn't exist conceptually for interaction
            }

            // Check player proximity and input
            float distanceToPlayer = Vector3.Distance(transform.position, roomManager.currentPlayer.transform.position);
            if (distanceToPlayer <= INTERACTION_DISTANCE && Input.GetKeyDown(interactionKey))
            {
                TryTraverse();
            }
        }

        void TryTraverse()
        {
            // Calculate the grid index of the room this door leads to
            Vector3Int nextRoomGridIndex = parentRoom.RoomIndex + leadsToWorldDirection;
            Room nextRoomScript = roomManager.GetRoomByGridIndex(nextRoomGridIndex);

            if (nextRoomScript != null) // If a room exists at the target location
            {
                // Optional: Use a FadeManager here if you have one for screen transitions
                // FadeManager.Instance.FadeOutIn(() => {
                    // Spawn player in the new room, passing the direction they moved in world space
                    roomManager.SpawnPlayerInRoom(nextRoomGridIndex, leadsToWorldDirection);
                    // Notify other systems (like Minimap) about the room change
                    roomManager.NotifyPlayerEnteredNewRoom(nextRoomGridIndex);
                // });
            }
            else
            {
                // This case should be rare if connector.isConnected is true, as it implies a room should be there.
                // Could happen if generation logic has a bug or if a room was unexpectedly destroyed.
                Debug.LogWarning($"DoorInteraction: Tried to traverse from {parentRoom.RoomIndex} via {leadsToWorldDirection} to {nextRoomGridIndex}, but no room script found at target. Dungeon might have an issue.");
            }
        }
    }
}