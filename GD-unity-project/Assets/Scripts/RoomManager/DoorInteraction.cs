using System.Collections;
using Helper;
using TMPro;
using UnityEngine;

namespace RoomManager
{
    public class DoorInteraction : MonoBehaviour
    {
        public Vector3Int leadsToWorldDirection;
        private RoomManager roomManager;
        private Room parentRoom;

        private const float INTERACTION_DISTANCE = 6f;
        public KeyCode interactionKey = KeyCode.E;
        
        private TextMeshProUGUI helpText;
        private GameObject helpTextContainer;

        // Audio management
		private GameObject player;
		private IEnumerator PlayDoorCloseAfterDelay(float delay) {
			yield return new WaitForSeconds(delay);
			AudioManager.instance.PlayOneShot(FMODEvents.instance.doorClose, player.transform.position);
		}
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && helpTextContainer != null && helpText != null)
            {
                helpText.text = "Press E to change room";
                helpTextContainer.SetActive(true);
            }   
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && helpTextContainer != null)
            {
                helpTextContainer.SetActive(false);
            }   
        }

		void Start()
        {
            roomManager = RoomManager.Instance;
            parentRoom = GetComponentInParent<Room>();
            
            helpTextContainer = GameObject.Find("FadeCanvasGroup").transform.Find("HelpTextContainer").gameObject;
            helpText = helpTextContainer.transform.Find("HelpText").GetComponent<TextMeshProUGUI>();

			// Audio management
			player = GameObject.Find("Player");

			if (roomManager == null)
                Debug.LogError("DoorInteraction: RoomManager.Instance not found!", this);
            if (parentRoom == null)
                Debug.LogError("DoorInteraction: Parent Room component not found!", this);

            if (leadsToWorldDirection == Vector3Int.zero && parentRoom != null)
            {
                InferDirectionFromName();
            }
        }

        private void InferDirectionFromName()
        {
            string myNameLower = gameObject.name.ToLower();
            if (myNameLower.Contains("north") || myNameLower.Contains("top"))
                leadsToWorldDirection = Vector3Int.forward;
            else if (myNameLower.Contains("south") || myNameLower.Contains("bottom"))
                leadsToWorldDirection = Vector3Int.back;
            else if (myNameLower.Contains("east") || myNameLower.Contains("right"))
                leadsToWorldDirection = Vector3Int.right;
            else if (myNameLower.Contains("west") || myNameLower.Contains("left"))
                leadsToWorldDirection = Vector3Int.left;
            else
                Debug.LogWarning($"DoorInteraction on '{gameObject.name}': Could not infer direction.", this);
        }

        void Update()
        {
            if (roomManager == null || parentRoom == null || roomManager.currentPlayer == null ||
                !roomManager.playerHasSpawned)
                return;
            if (leadsToWorldDirection == Vector3Int.zero)
                return;

            ConnectorDirection thisDoorsLocalConnectorDirection;
            if (leadsToWorldDirection == Vector3Int.forward)
                thisDoorsLocalConnectorDirection = ConnectorDirection.North;
            else if (leadsToWorldDirection == Vector3Int.back)
                thisDoorsLocalConnectorDirection = ConnectorDirection.South;
            else if (leadsToWorldDirection == Vector3Int.right)
                thisDoorsLocalConnectorDirection = ConnectorDirection.East;
            else if (leadsToWorldDirection == Vector3Int.left)
                thisDoorsLocalConnectorDirection = ConnectorDirection.West;
            else
            {
                Debug.LogError($"DoorInteraction on {gameObject.name}: Invalid direction {leadsToWorldDirection}.",
                    this);
                return;
            }

            RoomConnector connector = parentRoom.GetConnector(thisDoorsLocalConnectorDirection);

            if (connector == null || !connector.isConnected ||
                (connector.passageVisual != null && !connector.passageVisual.activeSelf))
                return;

            float distanceToPlayer = Vector3.Distance(transform.position, roomManager.currentPlayer.transform.position);

            if (distanceToPlayer <= INTERACTION_DISTANCE && Input.GetKeyDown(interactionKey))
            {
                TryTraverse();
            }
        }

        void TryTraverse()
        {
            Vector3Int nextRoomGridIndex = parentRoom.RoomIndex + leadsToWorldDirection;
            Room nextRoomScript = roomManager.GetRoomByGridIndex(nextRoomGridIndex);

            if (nextRoomScript != null)
            {
				// Audio management
				AudioManager.instance.PlayOneShot(FMODEvents.instance.doorOpen, this.transform.position);

				FadeManager.Instance.FadeOutIn(() =>
                {
                    roomManager.SpawnPlayerInRoom(nextRoomGridIndex, leadsToWorldDirection);
                    roomManager.NotifyPlayerEnteredNewRoom(nextRoomGridIndex);

					// Audio management
					StartCoroutine(PlayDoorCloseAfterDelay(0.5f)); // ritardo di 1,15 secondi
				});
            }
            else
            {
                Debug.LogWarning($"DoorInteraction: No room found at {nextRoomGridIndex}.");
            }
        }
    }
}