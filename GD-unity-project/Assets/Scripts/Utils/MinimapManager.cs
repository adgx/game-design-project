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

        [Tooltip("The parent Transform with a GridLayoutGroup that will contain all minimap icons.")] [SerializeField]
        private Transform _minimapGridParent;

        [Header("Minimap Colors")] [Tooltip("The color for a grid cell that does not contain a room.")] [SerializeField]
        private Color _emptyCellColor = new Color(0f, 0f, 0f, 0f);

        [SerializeField] private Sprite _currentRoomSprite;
        [SerializeField] private Sprite _visitedRoomSprite;
        [SerializeField] private Sprite _unvisitedRoomSprite;

        private Dictionary<Vector3Int, Image> _minimapIconsByGridIndex = new Dictionary<Vector3Int, Image>();
        private RoomManager.RoomManager _roomManager;
        private Vector3Int? _currentPlayerMinimapIndex;

        private void Awake()
        {
            _roomManager = RoomManager.RoomManager.Instance;

            if (_roomManager == null)
            {
                Debug.LogError("MinimapManager: RoomManager.Instance not found! Disabling MinimapManager.", this);
                enabled = false;
                return;
            }

            if (_minimapGridParent == null)
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
                    var name = $"MinimapCell_x{x}_z{z}";
                    GameObject iconGameObject = new GameObject(name, typeof(Image));
                    Image iconImage = iconGameObject.GetComponent<Image>();

                    if (_roomManager.DoesRoomExistAt(roomGridIndex))
                    {
                        iconImage.sprite = null;
                        iconImage.color = _emptyCellColor;
                        _minimapIconsByGridIndex[roomGridIndex] = iconImage;
                    }
                    else
                    {
                        Debug.LogError(
                            $"Minimap icon prefab '' is missing an Image component.", this);
                        Destroy(iconGameObject);
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
                    previousIcon.sprite = _visitedRoomSprite;
                }
            }

            if (_minimapIconsByGridIndex.TryGetValue(newRoomIndex, out Image currentIcon))
            {
                currentIcon.sprite = _currentRoomSprite;
            }

            _currentPlayerMinimapIndex = newRoomIndex;
        }
    }
}