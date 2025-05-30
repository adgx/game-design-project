using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RoomManager.MinimapManager
{
    public class MinimapManager : MonoBehaviour
    {
        [Header("Minimap UI References")]
        [SerializeField] private GameObject minimapRoomIconPrefab;
        [SerializeField] private Transform minimapGridParent;

        [Header("Minimap Colors")]
        [SerializeField] private Color emptyGridCellColor = new Color(0f, 0f, 0f, 0f);
        [SerializeField] private Color undiscoveredRoomColor = new Color(0.2f, 0.2f, 0.2f, 0f);
        [SerializeField] private Color visitedRoomColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        [SerializeField] private Color currentPlayerColor = Color.green;

        private Dictionary<Vector3Int, Image> minimapCellIcons = new Dictionary<Vector3Int, Image>();
        private RoomManager roomManagerInstance;
        private Vector3Int? currentPlayerRoomIndex;

        private void Start()
        {
            Debug.Log("MinimapManager: Start() called.");

            roomManagerInstance = FindFirstObjectByType<RoomManager>();
            if (!roomManagerInstance)
            {
                Debug.LogError("MinimapManager: RoomManager not found in the scene! Disabling MinimapManager.");
                enabled = false;
                return;
            }
            Debug.Log("MinimapManager: RoomManager instance found.");

            if (!minimapRoomIconPrefab)
            {
                Debug.LogError("MinimapManager: 'Minimap Room Icon Prefab' IS NOT ASSIGNED in the Inspector! Disabling MinimapManager.");
                enabled = false;
                return;
            }
            Debug.Log($"MinimapManager: 'Minimap Room Icon Prefab' is assigned: {minimapRoomIconPrefab.name}");

            if (!minimapGridParent)
            {
                Debug.LogError("MinimapManager: 'Minimap Grid Parent' IS NOT ASSIGNED in the Inspector! Disabling MinimapManager.");
                enabled = false;
                return;
            }
            Debug.Log($"MinimapManager: 'Minimap Grid Parent' is assigned: {minimapGridParent.name}");
        
            Debug.Log("MinimapManager: Subscribing to RoomManager events.");
            roomManagerInstance.OnRunReady += InitializeMinimap;
            roomManagerInstance.PlayerEnteredNewRoom += HandlePlayerEnteredRoom; 
        }

        private void OnDestroy()
        {
            if (!roomManagerInstance) return;
            Debug.Log("MinimapManager: Unsubscribing from RoomManager events.");
            roomManagerInstance.OnRunReady -= InitializeMinimap;
            roomManagerInstance.PlayerEnteredNewRoom -= HandlePlayerEnteredRoom;
        }

        private void InitializeMinimap()
        {
            Debug.Log("MinimapManager: InitializeMinimap() called.");

            if (!minimapRoomIconPrefab) {
                Debug.LogError("MinimapManager.InitializeMinimap: CRITICAL - minimapRoomIconPrefab is NULL. Aborting.");
                return; 
            }
            if (!minimapGridParent) {
                Debug.LogError("MinimapManager.InitializeMinimap: CRITICAL - minimapGridParent is NULL. Aborting.");
                return;
            }
            
            Debug.Log($"MinimapManager: Clearing old icons from {minimapGridParent.name}. Child count: {minimapGridParent.childCount}");
            foreach (Transform child in minimapGridParent) {
                if (child) Destroy(child.gameObject);
            }
            minimapCellIcons.Clear();
            currentPlayerRoomIndex = null;
            Debug.Log("MinimapManager: Old icons and state cleared.");
            
            int gridX = RoomManager.GetGridSizeX(); 
            int gridZ = RoomManager.GetGridSizeZ(); 
            Debug.Log($"MinimapManager: Initializing grid {gridX}x{gridZ}.");

            GridLayoutGroup gridLayout = minimapGridParent.GetComponent<GridLayoutGroup>();
            if (gridLayout) {
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = gridX;
            } else {
                Debug.LogWarning("MinimapManager: MinimapGridPanel is missing a GridLayoutGroup component. Layout might be incorrect.");
            }
            
            for (int zGame = gridZ - 1; zGame >= 0; zGame--)
            {
                for (int xGame = 0; xGame < gridX; xGame++)
                {
                    Vector3Int roomGridIndex = new Vector3Int(xGame, 0, zGame); 

                    GameObject iconGo = Instantiate(minimapRoomIconPrefab, minimapGridParent); 
                    iconGo.name = $"MinimapCell_x{xGame}_z{zGame}";
                    Image iconImage = iconGo.GetComponent<Image>();

                    if (iconImage)
                    {
                        iconImage.color = roomManagerInstance.DoesRoomExistAt(roomGridIndex) ? undiscoveredRoomColor : emptyGridCellColor;
                        minimapCellIcons[roomGridIndex] = iconImage;
                    } else {
                        Debug.LogError($"MinimapRoomIconPrefab '{minimapRoomIconPrefab.name}' is missing an Image component. Destroying icon instance.");
                        if (iconGo) Destroy(iconGo);
                    }
                }
            }
            Debug.Log("MinimapManager: Grid icons instantiated.");
            
            if (roomManagerInstance.PlayerHasSpawned) {
                Debug.Log($"MinimapManager: Player already spawned in {roomManagerInstance.CurrentRoomIndex}, updating minimap for it.");
                HandlePlayerEnteredRoom(roomManagerInstance.CurrentRoomIndex);
            } else {
                Debug.Log("MinimapManager: Player not yet spawned by RoomManager, waiting for PlayerEnteredNewRoom or next OnRunReady if player spawns late.");
            }
            Debug.Log("MinimapManager: InitializeMinimap() finished.");
        }

        private void HandlePlayerEnteredRoom(Vector3Int newRoomIndex)
        {
            Debug.Log($"MinimapManager: HandlePlayerEnteredRoom({newRoomIndex}). Previous player room: {currentPlayerRoomIndex?.ToString() ?? "None"}");
            
            if (currentPlayerRoomIndex.HasValue && currentPlayerRoomIndex.Value != newRoomIndex)
            {
                if (minimapCellIcons.TryGetValue(currentPlayerRoomIndex.Value, out Image prevPlayerIcon))
                {
                    if (prevPlayerIcon && roomManagerInstance.DoesRoomExistAt(currentPlayerRoomIndex.Value)) {
                        prevPlayerIcon.color = visitedRoomColor;
                    } else if (!prevPlayerIcon) {
                        Debug.LogWarning($"MinimapManager: Previous player icon for {currentPlayerRoomIndex.Value} was null in dictionary (already destroyed or error).");
                    }
                }
            }
            
            if (minimapCellIcons.TryGetValue(newRoomIndex, out Image newPlayerIcon))
            {
                if (newPlayerIcon) {
                    newPlayerIcon.color = currentPlayerColor;
                } else {
                    Debug.LogWarning($"MinimapManager: New player icon for {newRoomIndex} was null in dictionary (already destroyed or error).");
                }
            }
            else
            {
                Debug.LogWarning($"MinimapManager: Minimap icon for new room {newRoomIndex} not found in dictionary. Was it created during InitializeMinimap?");
            }
            
            currentPlayerRoomIndex = newRoomIndex;
            Debug.Log($"MinimapManager: Current player room icon updated to {newRoomIndex}. Minimap should reflect this.");
        }
    }
}