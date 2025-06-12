using System.Collections.Generic;
using RoomManager;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    /// <summary>
    /// Manages the creation and state of the UI minimap based on the generated dungeon layout.
    /// </summary>
    public class MinimapManager : MonoBehaviour
    {
        [Header("Minimap UI References")]
        [Tooltip("The UI Image prefab used to represent a single room on the minimap.")]
        [SerializeField]
        private GameObject _minimapIconPrefab;

        [Tooltip("The parent Transform with a GridLayoutGroup that will contain all minimap icons.")] [SerializeField]
        private Transform _minimapGridParent;

        [Header("Minimap Colors")] [Tooltip("The color for a grid cell that does not contain a room.")] [SerializeField]
        private Color _emptyCellColor = new Color(0f, 0f, 0f, 0f);

        [Tooltip("The color for a room that exists but has not been visited by the player.")] [SerializeField]
        private Color _undiscoveredRoomColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        [Tooltip("The color for a room that the player has already visited.")] [SerializeField]
        private Color _visitedRoomColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        [Tooltip("The color for the icon representing the player's current room.")] [SerializeField]
        private Color _currentPlayerColor = Color.green;

        private Dictionary<Vector3Int, Image> _minimapIconsByGridIndex = new Dictionary<Vector3Int, Image>();
        private RoomManager.RoomManager _roomManager;
        private Vector3Int? _currentPlayerMinimapIndex;

        private void Start()
        {
            _roomManager = RoomManager.RoomManager.Instance;
            if (_roomManager == null)
            {
                Debug.LogError("MinimapManager: RoomManager.Instance not found! Disabling MinimapManager.", this);
                enabled = false;
                return;
            }

            if (_minimapIconPrefab == null || _minimapGridParent == null)
            {
                Debug.LogError("MinimapManager: UI Prefab or Grid Parent not assigned! Disabling MinimapManager.",
                    this);
                enabled = false;
                return;
            }

            _roomManager.OnRunReady += InitializeMinimap;
            _roomManager.PlayerEnteredNewRoom += HandlePlayerEnteredRoomOnMinimap;
        }

        private void OnDestroy()
        {
            if (_roomManager != null)
            {
                _roomManager.OnRunReady -= InitializeMinimap;
                _roomManager.PlayerEnteredNewRoom -= HandlePlayerEnteredRoomOnMinimap;
            }
        }

        private void InitializeMinimap()
        {
            foreach (Transform child in _minimapGridParent)
            {
                Destroy(child.gameObject);
            }

            _minimapIconsByGridIndex.Clear();
            _currentPlayerMinimapIndex = null;

            int gridWidth = _roomManager.GetGridSizeX();
            int gridHeight = _roomManager.GetGridSizeZ();

            if (_minimapGridParent.GetComponent<GridLayoutGroup>() is { } gridLayout)
            {
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = gridWidth;
            }
            else
            {
                Debug.LogWarning(
                    "MinimapManager: MinimapGridParent is missing a GridLayoutGroup. Layout may be incorrect.", this);
            }

            for (int z = gridHeight - 1; z >= 0; z--)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    var roomGridIndex = new Vector3Int(x, 0, z);
                    GameObject iconGO = Instantiate(_minimapIconPrefab, _minimapGridParent);
                    iconGO.name = $"MinimapCell_x{x}_z{z}";

                    if (iconGO.GetComponent<Image>() is { } iconImage)
                    {
                        iconImage.color = _roomManager.DoesRoomExistAt(roomGridIndex)
                            ? _undiscoveredRoomColor
                            : _emptyCellColor;
                        _minimapIconsByGridIndex[roomGridIndex] = iconImage;
                    }
                    else
                    {
                        Debug.LogError(
                            $"Minimap icon prefab '{_minimapIconPrefab.name}' is missing an Image component.", this);
                        Destroy(iconGO);
                    }
                }
            }

            if (_roomManager.IsPlayerSpawned)
            {
                HandlePlayerEnteredRoomOnMinimap(_roomManager.CurrentRoomIndex);
            }
        }

        private void HandlePlayerEnteredRoomOnMinimap(Vector3Int newRoomIndex)
        {
            if (_currentPlayerMinimapIndex.HasValue && _currentPlayerMinimapIndex.Value != newRoomIndex)
            {
                if (_minimapIconsByGridIndex.TryGetValue(_currentPlayerMinimapIndex.Value, out Image previousIcon))
                {
                    previousIcon.color = _visitedRoomColor;
                }
            }

            if (_minimapIconsByGridIndex.TryGetValue(newRoomIndex, out Image currentIcon))
            {
                currentIcon.color = _currentPlayerColor;
            }

            _currentPlayerMinimapIndex = newRoomIndex;
        }
    }
}