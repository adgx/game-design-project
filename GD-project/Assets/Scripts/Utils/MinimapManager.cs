using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public class MinimapManager : MonoBehaviour
    {
        [Header("Minimap UI References")] [SerializeField]
        private GameObject minimapRoomIconPrefab;

        [SerializeField] private Transform minimapGridParent;

        [Header("Minimap Colors")] [SerializeField]
        private Color emptyGridCellColor = new Color(0f, 0f, 0f, 0f);

        [SerializeField] private Color undiscoveredRoomColor = new Color(0.2f, 0.2f, 0.2f, 0f);
        [SerializeField] private Color visitedRoomColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        [SerializeField] private Color currentPlayerColor = Color.green;

        private Dictionary<Vector3Int, Image> minimapCellIcons = new Dictionary<Vector3Int, Image>();
        private global::RoomManager.RoomManager roomManagerInstance;
        private Vector3Int? currentPlayerRoomIndexOnMinimap = null;

        void Start()
        {
            roomManagerInstance = global::RoomManager.RoomManager.Instance;
            if (roomManagerInstance == null)
            {
                Debug.LogError("MinimapManager: RoomManager.Instance not found! Disabling MinimapManager.");
                enabled = false;
                return;
            }

            if (minimapRoomIconPrefab == null || minimapGridParent == null)
            {
                Debug.LogError("MinimapManager: UI Prefab or Grid Parent not assigned! Disabling MinimapManager.");
                enabled = false;
                return;
            }

            roomManagerInstance.OnRunReady += InitializeMinimap;
            roomManagerInstance.PlayerEnteredNewRoom += HandlePlayerEnteredRoomOnMinimap;
        }

        private void OnDestroy()
        {
            if (roomManagerInstance != null)
            {
                roomManagerInstance.OnRunReady -= InitializeMinimap;
                roomManagerInstance.PlayerEnteredNewRoom -= HandlePlayerEnteredRoomOnMinimap;
            }
        }

        private void InitializeMinimap()
        {
            foreach (Transform child in minimapGridParent)
            {
                if (child != null) Destroy(child.gameObject);
            }

            minimapCellIcons.Clear();
            currentPlayerRoomIndexOnMinimap = null;

            int gridX = roomManagerInstance.GetGridSizeX();
            int gridZ = roomManagerInstance.GetGridSizeZ();

            GridLayoutGroup gridLayout = minimapGridParent.GetComponent<GridLayoutGroup>();
            if (gridLayout != null)
            {
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = gridX;
            }
            else
            {
                Debug.LogWarning(
                    "MinimapManager: MinimapGridParent is missing GridLayoutGroup. Layout may be incorrect.");
            }

            for (int z_game = gridZ - 1; z_game >= 0; z_game--)
            {
                for (int x_game = 0; x_game < gridX; x_game++)
                {
                    Vector3Int roomGridIndex = new Vector3Int(x_game, 0, z_game);
                    GameObject iconGO = Instantiate(minimapRoomIconPrefab, minimapGridParent);
                    iconGO.name = $"MinimapCell_x{x_game}_z{z_game}";
                    Image iconImage = iconGO.GetComponent<Image>();

                    if (iconImage != null)
                    {
                        iconImage.color = roomManagerInstance.DoesRoomExistAt(roomGridIndex)
                            ? undiscoveredRoomColor
                            : emptyGridCellColor;
                        minimapCellIcons[roomGridIndex] = iconImage;
                    }
                    else
                    {
                        Debug.LogError(
                            $"Minimap icon prefab {minimapRoomIconPrefab.name} is missing an Image component.");
                        if (iconGO != null) Destroy(iconGO);
                    }
                }
            }

            if (roomManagerInstance.playerHasSpawned)
            {
                HandlePlayerEnteredRoomOnMinimap(roomManagerInstance.CurrentRoomIndex);
            }
        }

        private void HandlePlayerEnteredRoomOnMinimap(Vector3Int newRoomIndex)
        {
            if (currentPlayerRoomIndexOnMinimap.HasValue && currentPlayerRoomIndexOnMinimap.Value != newRoomIndex)
            {
                if (minimapCellIcons.TryGetValue(currentPlayerRoomIndexOnMinimap.Value, out Image prevIcon) &&
                    prevIcon != null)
                {
                    if (roomManagerInstance.DoesRoomExistAt(currentPlayerRoomIndexOnMinimap.Value))
                        prevIcon.color = visitedRoomColor;
                }
            }

            if (minimapCellIcons.TryGetValue(newRoomIndex, out Image currentIcon) && currentIcon != null)
            {
                currentIcon.color = currentPlayerColor;
            }

            currentPlayerRoomIndexOnMinimap = newRoomIndex;
        }
    }
}