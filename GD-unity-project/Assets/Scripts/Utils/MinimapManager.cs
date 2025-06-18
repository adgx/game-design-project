using System.Collections.Generic;
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
        [Tooltip("The parent Transform with a GridLayoutGroup that will contain all minimap icons.")]
        [SerializeField]
        private Transform _minimapGridParent;

        [Tooltip("A prefab for a single minimap cell. Should have an Image component.")] [SerializeField]
        private GameObject _minimapCellPrefab;

        [SerializeField] private Sprite _currentRoomSprite;
        [SerializeField] private Sprite _visitedRoomSprite;
        [SerializeField] private Sprite _unvisitedRoomSprite;

        private readonly Dictionary<Vector3Int, Image> _minimapIconsByGridIndex = new Dictionary<Vector3Int, Image>();
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

            if (_minimapGridParent == null || _minimapCellPrefab == null)
            {
                Debug.LogError("MinimapManager: Grid Parent or Cell Prefab not assigned! Disabling MinimapManager.",
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
                gridLayout.spacing = new Vector2(5f, 5f);
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

                    GameObject iconGameObject = Instantiate(_minimapCellPrefab, _minimapGridParent);
                    iconGameObject.name = $"MinimapCell_x{x}_z{z}";

                    Image iconImage = iconGameObject.GetComponent<Image>();

                    if (iconImage == null)
                    {
                        Debug.LogError($"Minimap Cell Prefab is missing an Image component on '{iconGameObject.name}'");
                        continue;
                    }

                    if (_roomManager.DoesRoomExistAt(roomGridIndex))
                    {
                        iconImage.sprite = _unvisitedRoomSprite;
                        iconImage.color = Color.clear;
                        _minimapIconsByGridIndex[roomGridIndex] = iconImage;
                    }
                    else
                    {
                        iconImage.sprite = null;
                        iconImage.color = Color.clear;
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
                    previousIcon.color = Color.white;
                }
            }

            if (_minimapIconsByGridIndex.TryGetValue(newRoomIndex, out Image currentIcon))
            {
                currentIcon.sprite = _currentRoomSprite;
                currentIcon.color = Color.white;
            }

            _currentPlayerMinimapIndex = newRoomIndex;
        }
    }
}