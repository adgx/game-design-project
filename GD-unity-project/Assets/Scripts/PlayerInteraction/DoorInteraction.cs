using System.Threading.Tasks;
using Helper;
using RoomManager;
using TMPro;
using UnityEngine;

namespace PlayerInteraction
{
    /// <summary>
    /// Handles player interaction with a door to traverse to an adjacent room.
    /// </summary>
    public class DoorInteraction : MonoBehaviour, IInteractable
    {
        /// <summary>
        /// The prompt text shown to the player when they are near this door.
        /// </summary>
        public string InteractionPrompt => $"Press E to use door";

        public bool IsInteractable => !_isTraversing;

        private GameObject player;

        [Header("Door Configuration")]
        [Tooltip(
            "The world direction this door leads to (e.g., Vector3Int.forward for North). Inferred from name if left at zero.")]
        [SerializeField]
        private Vector3Int _leadsToWorldDirection;

        private RoomManager.RoomManager _roomManager;
        private Room _parentRoom;
        private bool _isTraversing = false;

        private GameObject helpTextContainer;
        private TextMeshProUGUI helpText;

        /// <summary>
        /// Initializes door references and optionally infers direction from the GameObject's name.
        /// </summary>
        private void Start()
        {
            _roomManager = RoomManager.RoomManager.Instance;
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

        /// <summary>
        /// Called when the player interacts with the door.
        /// Triggers room traversal if the door is connected.
        /// </summary>
        /// <param name="interactor">The GameObject attempting interaction.</param>
        /// <returns>True if interaction was successful.</returns>
        public bool Interact(GameObject interactor)
        {
            if (_isTraversing) return false;

            ConnectorDirection thisDoorsLocalConnectorDirection =
                RoomManager.RoomManager.GetOppositeLocalDirection(_leadsToWorldDirection * -1);
            RoomConnector connector = _parentRoom.GetConnector(thisDoorsLocalConnectorDirection);

            if (connector == null || !connector.IsConnected)
            {
                Debug.LogWarning("This door leads nowhere.", this);
                return false;
            }

            _ = TryTraverse(interactor);
            return true;
        }

        /// <summary>
        /// Attempts to traverse to the next room in the specified direction with fade and delay effects.
        /// </summary>
        /// <param name="interactor">The GameObject initiating the traversal.</param>
        private async Task TryTraverse(GameObject interactor)
        {
            _isTraversing = true;

            Vector3Int nextRoomGridIndex = _parentRoom.RoomIndex + _leadsToWorldDirection;

            if (_roomManager.DoesRoomExistAt(nextRoomGridIndex))
            {
                GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.doorOpen, interactor.transform.position);

                await Task.Delay(1000);

                FadeManager.Instance.FadeOutIn(() =>
                {
                    _roomManager.TraverseRoom(nextRoomGridIndex, _leadsToWorldDirection);
                    GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.doorClose,
                        interactor.transform.position);
                });
            }
            else
            {
                Debug.LogWarning(
                    $"DoorInteraction: Tried to traverse to {nextRoomGridIndex}, but no room exists there.");
            }

            await Task.Delay(2000);
            _isTraversing = false;
        }

        /// <summary>
        /// Tries to infer the world direction this door leads to based on its GameObject name.
        /// </summary>
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
    }
}