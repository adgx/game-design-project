using Helper;
using UnityEngine;
using Utils;

namespace RoomManager
{
    /// <summary>
    /// Handles player interaction with a door to traverse to an adjacent room.
    /// </summary>
    public class DoorInteraction : MonoBehaviour
    {
        [Header("Door Configuration")]
        [Tooltip(
            "The world direction this door leads to (e.g., Vector3Int.forward for North). Inferred from name if left at zero.")]
        [SerializeField]
        private Vector3Int _leadsToWorldDirection;

        [Tooltip("The distance within which the player can interact with this door.")] [SerializeField]
        private float _interactionDistance = 6f;

        [Tooltip("The key the player must press to use the door.")] [SerializeField]
        private KeyCode _interactionKey = KeyCode.E;

        private RoomManager _roomManager;
        private Room _parentRoom;

        private void Start()
        {
            _roomManager = RoomManager.Instance;
            _parentRoom = GetComponentInParent<Room>();

            if (_roomManager == null)
                Debug.LogError("DoorInteraction: RoomManager.Instance not found!", this);
            if (_parentRoom == null)
                Debug.LogError(
                    "DoorInteraction: Parent Room component not found! This object must be a child of a Room.", this);

            if (_leadsToWorldDirection == Vector3Int.zero && _parentRoom != null)
            {
                InferDirectionFromName();
            }
        }

        private void InferDirectionFromName()
        {
            string myNameLower = gameObject.name.ToLower();
            if (myNameLower.Contains("north") || myNameLower.Contains("top"))
                _leadsToWorldDirection = Vector3Int.forward;
            else if (myNameLower.Contains("south") || myNameLower.Contains("bottom"))
                _leadsToWorldDirection = Vector3Int.back;
            else if (myNameLower.Contains("east") || myNameLower.Contains("right"))
                _leadsToWorldDirection = Vector3Int.right;
            else if (myNameLower.Contains("west") || myNameLower.Contains("left"))
                _leadsToWorldDirection = Vector3Int.left;
            else
                Debug.LogWarning($"DoorInteraction on '{gameObject.name}': Could not infer direction from name.", this);
        }

        private void Update()
        {
            if (_roomManager == null || _parentRoom == null || _roomManager.CurrentPlayer == null ||
                !_roomManager.IsPlayerSpawned)
                return;

            if (_leadsToWorldDirection == Vector3Int.zero)
                return;

            ConnectorDirection thisDoorsLocalConnectorDirection =
                RoomManager.GetOppositeLocalDirection(_leadsToWorldDirection * -1);
            RoomConnector connector = _parentRoom.GetConnector(thisDoorsLocalConnectorDirection);

            if (connector == null || !connector.IsConnected)
                return;

            float distanceToPlayer =
                Vector3.Distance(transform.position, _roomManager.CurrentPlayer.transform.position);

            if (distanceToPlayer <= _interactionDistance && Input.GetKeyDown(_interactionKey))
            {
                TryTraverse();
            }
        }

        private void TryTraverse()
        {
            Vector3Int nextRoomGridIndex = _parentRoom.RoomIndex + _leadsToWorldDirection;

            if (_roomManager.DoesRoomExistAt(nextRoomGridIndex))
            {
                FadeManager.Instance.FadeOutIn(() =>
                {
                    _roomManager.TraverseRoom(nextRoomGridIndex, _leadsToWorldDirection);
                });
            }
            else
            {
                Debug.LogWarning(
                    $"DoorInteraction: Tried to traverse to {nextRoomGridIndex}, but no room exists there.");
            }
        }
    }
}