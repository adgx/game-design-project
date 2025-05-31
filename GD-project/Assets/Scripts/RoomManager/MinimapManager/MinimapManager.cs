// MinimapManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RoomManager; // Assuming RoomManager namespace

public class MinimapManager : MonoBehaviour
{
    [Header("Minimap UI References")]
    [SerializeField] private GameObject minimapRoomIconPrefab;
    [SerializeField] private Transform minimapGridParent;

    [Header("Minimap Colors")]
    [SerializeField] private Color emptyGridCellColor = new Color(0f, 0f, 0f, 0f);      // Fully transparent
    [SerializeField] private Color undiscoveredRoomColor = new Color(0.2f, 0.2f, 0.2f, 0f); // Fully transparent for existing but unvisited
    [SerializeField] private Color visitedRoomColor = new Color(0.7f, 0.7f, 0.7f, 1f);     // Opaque light gray
    [SerializeField] private Color currentPlayerColor = Color.green;                       // Opaque green

    private Dictionary<Vector3Int, Image> minimapCellIcons = new Dictionary<Vector3Int, Image>();
    private RoomManager.RoomManager roomManagerInstance;
    private Vector3Int? currentPlayerRoomIndexOnMinimap = null; // Tracks player's room for minimap updates

    void Start()
    {
        roomManagerInstance = RoomManager.RoomManager.Instance; // Use the singleton
        if (roomManagerInstance == null) {
            Debug.LogError("MinimapManager: RoomManager.Instance not found! Disabling MinimapManager.");
            enabled = false; return;
        }
        if (minimapRoomIconPrefab == null || minimapGridParent == null) {
            Debug.LogError("MinimapManager: UI Prefab or Grid Parent not assigned! Disabling MinimapManager.");
            enabled = false; return;
        }
        
        // Subscribe to RoomManager events
        roomManagerInstance.OnRunReady += InitializeMinimap;
        roomManagerInstance.PlayerEnteredNewRoom += HandlePlayerEnteredRoomOnMinimap;
    }

    private void OnDestroy()
    {
        if (roomManagerInstance != null) { // Unsubscribe to prevent errors if RoomManager is destroyed first
            roomManagerInstance.OnRunReady -= InitializeMinimap;
            roomManagerInstance.PlayerEnteredNewRoom -= HandlePlayerEnteredRoomOnMinimap;
        }
    }

    private void InitializeMinimap()
    {
        // Clear existing minimap icons
        foreach (Transform child in minimapGridParent) { if (child != null) Destroy(child.gameObject); }
        minimapCellIcons.Clear();
        currentPlayerRoomIndexOnMinimap = null;

        int gridX = roomManagerInstance.GetGridSizeX();
        int gridZ = roomManagerInstance.GetGridSizeZ();

        // Configure GridLayoutGroup for proper icon arrangement
        GridLayoutGroup gridLayout = minimapGridParent.GetComponent<GridLayoutGroup>();
        if (gridLayout != null) {
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = gridX;
        } else {
            Debug.LogWarning("MinimapManager: MinimapGridParent is missing GridLayoutGroup. Layout may be incorrect.");
        }

        // Populate minimap (iterate Z in reverse to match game's coordinate system if using Unity's default UI top-left origin)
        for (int z_game = gridZ - 1; z_game >= 0; z_game--) { // Iterating "game Z" from max down to 0
            for (int x_game = 0; x_game < gridX; x_game++) {  // Iterating "game X" from 0 to max
                Vector3Int roomGridIndex = new Vector3Int(x_game, 0, z_game); // Construct grid index
                GameObject iconGO = Instantiate(minimapRoomIconPrefab, minimapGridParent);
                iconGO.name = $"MinimapCell_x{x_game}_z{z_game}";
                Image iconImage = iconGO.GetComponent<Image>();

                if (iconImage != null) {
                    // Set initial color based on whether a room exists at this grid index
                    iconImage.color = roomManagerInstance.DoesRoomExistAt(roomGridIndex) ?
                                      undiscoveredRoomColor : emptyGridCellColor;
                    minimapCellIcons[roomGridIndex] = iconImage; // Store reference to the icon's Image component
                } else {
                    Debug.LogError($"Minimap icon prefab {minimapRoomIconPrefab.name} is missing an Image component.");
                    if (iconGO != null) Destroy(iconGO); // Cleanup malformed icon instance
                }
            }
        }
        
        // If player has already spawned by the time minimap initializes, update their current room color
        if (roomManagerInstance.playerHasSpawned) {
            HandlePlayerEnteredRoomOnMinimap(roomManagerInstance.CurrentRoomIndex);
        }
    }

    private void HandlePlayerEnteredRoomOnMinimap(Vector3Int newRoomIndex)
    {
        // Update color of the previously current room (if any and if it's different from the new room)
        if (currentPlayerRoomIndexOnMinimap.HasValue && currentPlayerRoomIndexOnMinimap.Value != newRoomIndex) {
            if (minimapCellIcons.TryGetValue(currentPlayerRoomIndexOnMinimap.Value, out Image prevIcon) && prevIcon != null) {
                // Only set to 'visited' if it was a valid room (not an empty cell that was erroneously colored)
                if(roomManagerInstance.DoesRoomExistAt(currentPlayerRoomIndexOnMinimap.Value))
                    prevIcon.color = visitedRoomColor;
            }
        }

        // Update color of the new current room
        if (minimapCellIcons.TryGetValue(newRoomIndex, out Image currentIcon) && currentIcon != null) {
            currentIcon.color = currentPlayerColor;
        }
        currentPlayerRoomIndexOnMinimap = newRoomIndex; // Track the new current room for the minimap
    }
}