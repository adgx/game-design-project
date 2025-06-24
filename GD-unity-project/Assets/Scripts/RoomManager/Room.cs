using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace RoomManager
{
    /// <summary>
    /// Defines the cardinal directions a connector can face.
    /// </summary>
    public enum ConnectorDirection
    {
        North,
        South,
        East,
        West
    }

    /// <summary>
    /// Represents a connector (e.g., doorway) that can link rooms together.
    /// </summary>
    [System.Serializable]
    public class RoomConnector
    {
        [Tooltip("An optional identifier used to distinguish this connector.")] [SerializeField]
        private string _id;

        [Tooltip("The direction this connector leads to, relative to the room.")] [SerializeField]
        private ConnectorDirection _direction;

        [Tooltip("The visual GameObject representing the passage (e.g., a doorway).")] [SerializeField]
        private GameObject _passageVisual;

        [Tooltip("The spawn point for the player when entering from this connector.")] [SerializeField]
        private Transform _spawnPoint;

        /// <summary>
        /// Gets the identifier for this connector.
        /// </summary>
        public string ID => _id;

        /// <summary>
        /// Gets the direction this connector faces.
        /// </summary>
        public ConnectorDirection Direction => _direction;

        /// <summary>
        /// Gets the visual GameObject used for this connector's passage.
        /// </summary>
        public GameObject PassageVisual => _passageVisual;

        /// <summary>
        /// Gets or sets the spawn point linked to this connector.
        /// </summary>
        public Transform SpawnPoint { get; set; }

        /// <summary>
        /// Indicates whether this connector has been linked to another room.
        /// </summary>
        [HideInInspector] public bool IsConnected = false;
    }

    /// <summary>
    /// Represents a single room within the dungeon system.
    /// </summary>
    public class Room : MonoBehaviour, IRoom
    {
        [Header("Room Properties")]
        [Tooltip("All possible connectors that can link this room to others.")]
        [SerializeField]
        private List<RoomConnector> _connectors = new();

        [Tooltip("Default spawn point used if no directional spawn point is available.")] [SerializeField]
        private Transform _centralSpawnPoint;

        [Tooltip("Points in the room where enemies can spawn.")] [SerializeField]
        private List<Transform> _enemySpawnPoints;

        [Tooltip("Prefab for a vending machine that may spawn in the room.")] [SerializeField]
        private GameObject _vendingMachinePrefab;

        [Tooltip("Chance (0–1) for a vending machine to spawn.")] [Range(0f, 1f)] [SerializeField]
        private float _vendingMachineSpawnChance = 0.5f;

        [Tooltip("Whether a vending machine is allowed to spawn in this room.")] [SerializeField]
        private bool _canVendingMachineSpawn;

        [Tooltip("Prefab for an upgrade terminal that may spawn in the room.")] [SerializeField]
        private GameObject _upgradeTerminalPrefab;

        [Tooltip("Chance (0–1) for an upgrade terminal to spawn.")] [Range(0f, 1f)] [SerializeField]
        private float _upgradeTerminalSpawnChance = 0.5f;

        [Tooltip("Whether an upgrade terminal is allowed to spawn in this room.")] [SerializeField]
        private bool _canUpgradeTerminalSpawn;

        [Tooltip("Prefab for a collectible paper item that may appear in the room.")] [SerializeField]
        private GameObject _paperPrefab;

        [Tooltip("Chance (0–1) for the paper to spawn.")] [Range(0f, 1f)] [SerializeField]
        private float _paperSpawnChance = 0.1f;

        [Tooltip("Whether the paper is allowed to spawn in this room.")] [SerializeField]
        private bool _canPaperSpawn;

        private RoomManager _roomManager;

        /// <summary>
        /// Gets the type of this room.
        /// </summary>
        public RoomType RoomType { get; private set; }

        /// <summary>
        /// Gets the maximum allowed cost for enemy spawning in this room.
        /// </summary>
        public int MaxSpawnCost { get; private set; } = 10;

        /// <summary>
        /// Gets or sets the room’s position within the dungeon grid.
        /// </summary>
        public Vector3Int RoomIndex { get; set; }

        /// <summary>
        /// Indicates whether enemies have already spawned in this room.
        /// </summary>
        public bool HasEnemiesSpawned { get; set; }

        /// <summary>
        /// Gets the fallback central spawn point.
        /// </summary>
        public Transform CentralSpawnPoint => _centralSpawnPoint;

        /// <summary>
        /// Gets a read-only list of enemy spawn points.
        /// </summary>
        public IReadOnlyList<Transform> EnemySpawnPoints => _enemySpawnPoints;

        /// <summary>
        /// Initializes this room with data and references.
        /// </summary>
        /// <param name="roomData">Data used to configure the room.</param>
        /// <param name="roomManager">Reference to the room manager.</param>
        public void Initialize(RoomData.RoomData roomData, RoomManager roomManager)
        {
            this._roomManager = roomManager;

            if (roomData == null) return;

            MaxSpawnCost = roomData.roomSpawnBudget;
            RoomType = roomData.roomType;
        }

        private void Awake()
        {
            if (_centralSpawnPoint == null)
            {
                GameObject centralSpawnPointGameObject = new GameObject("CentralSpawnPoint_Generated");
                centralSpawnPointGameObject.transform.SetParent(transform);
                centralSpawnPointGameObject.transform.localPosition = Vector3.zero;
                _centralSpawnPoint = centralSpawnPointGameObject.transform;
            }

            foreach (var connector in _connectors)
            {
                if (connector.PassageVisual != null)
                {
                    connector.PassageVisual.SetActive(false);
                }

                connector.IsConnected = false;
                
                if (connector.SpawnPoint == null)
                {
                    connector.SpawnPoint = CreateSpawnPointForConnector(connector);
                }
            }
        }

        /// <summary>
        /// Creates a spawn point for a connector if one doesn't exist.
        /// </summary>
        private Transform CreateSpawnPointForConnector(RoomConnector connector)
        {
            GameObject spawnPointGameObject = new GameObject($"{connector.Direction}SpawnPoint_Generated");
            spawnPointGameObject.transform.SetParent(transform);

            Vector3 spawnLocalPos;
            const float offset = 1.5f;

            if (connector.PassageVisual != null)
            {
                Vector3 inwardLocalDir = -RoomManager.GetWorldVectorFromLocalDirection(connector.Direction, transform)
                    .normalized;
                spawnLocalPos = connector.PassageVisual.transform.localPosition + (inwardLocalDir * offset);
            }
            else
            {
                float halfWidth = _roomManager != null ? RoomManager.GetCellWorldWidth() / 2f : 100f;
                float halfDepth = _roomManager != null ? RoomManager.GetCellWorldDepth() / 2f : 100f;

                spawnLocalPos = connector.Direction switch
                {
                    ConnectorDirection.North => new Vector3(0, 0, halfDepth - offset),
                    ConnectorDirection.South => new Vector3(0, 0, -halfDepth + offset),
                    ConnectorDirection.East => new Vector3(halfWidth - offset, 0, 0),
                    ConnectorDirection.West => new Vector3(-halfWidth + offset, 0, 0),
                    _ => Vector3.zero
                };
            }

            spawnPointGameObject.transform.localPosition = spawnLocalPos;
            return spawnPointGameObject.transform;
        }

        /// <summary>
        /// Returns the connector in the specified direction.
        /// </summary>
        public RoomConnector GetConnector(ConnectorDirection direction)
        {
            return _connectors.Find(c => c.Direction == direction);
        }

        /// <summary>
        /// Returns all unconnected connectors in this room.
        /// </summary>
        public List<RoomConnector> GetAvailableConnectors()
        {
            return _connectors.Where(c => !c.IsConnected).ToList();
        }

        /// <summary>
        /// Activates a connection in the specified direction.
        /// </summary>
        private void ActivateConnection(ConnectorDirection direction)
        {
            RoomConnector connector = GetConnector(direction);
            if (connector != null)
            {
                if (connector.PassageVisual != null)
                    connector.PassageVisual.SetActive(true);

                connector.IsConnected = true;
            }
        }

        /// <summary>
        /// Gets the appropriate spawn point based on entry direction.
        /// </summary>
        /// <param name="worldEntryDirection">The direction the player entered the room from.</param>
        public Transform GetSpawnPointForWorldEntryDirection(Vector3Int worldEntryDirection)
        {
            ConnectorDirection entrySide = RoomManager.GetOppositeLocalDirection(worldEntryDirection);
            RoomConnector connector = GetConnector(entrySide);

            return connector?.SpawnPoint ?? _centralSpawnPoint;
        }

        /// <summary>
        /// Activates connections to adjacent rooms.
        /// </summary>
        public void PostInitializeConnections()
        {
            foreach (ConnectorDirection dir in System.Enum.GetValues(typeof(ConnectorDirection)))
            {
                Vector3Int neighbor = RoomIndex + RoomManager.GetVectorFromLocalDirection(dir);
                if (_roomManager.DoesRoomExistAt(neighbor))
                {
                    ActivateConnection(dir);
                }
            }
        }

        /// <summary>
        /// Attempts to spawn the vending machine based on chance and eligibility.
        /// </summary>
        public bool PostInitializeVendingMachine()
        {
            if (!_canVendingMachineSpawn) return false;

            bool spawned = Random.value < _vendingMachineSpawnChance;
            _vendingMachinePrefab.SetActive(spawned);
            return spawned;
        }

        /// <summary>
        /// Attempts to spawn the upgrade terminal based on chance and eligibility.
        /// </summary>
        public bool PostInitializeUpgradeTerminal()
        {
            if (!_canUpgradeTerminalSpawn) return false;

            bool spawned = Random.value < _upgradeTerminalSpawnChance;
            _upgradeTerminalPrefab.SetActive(spawned);
            return spawned;
        }

        /// <summary>
        /// Attempts to spawn a collectible paper based on chance and eligibility.
        /// </summary>
        public bool PostInitializePaper()
        {
            if (!_canPaperSpawn) return false;

            bool spawned = Random.value < _paperSpawnChance;
            _paperPrefab.SetActive(spawned);

            print(spawned);

            if (!spawned)
            {
                _paperSpawnChance *= 1.3f;
            }
            
            return spawned;
        }

        private void OnDrawGizmosSelected()
        {
            float width = Application.isPlaying && RoomManager.Instance != null
                ? RoomManager.GetCellWorldWidth()
                : 200f;
            float depth = Application.isPlaying && RoomManager.Instance != null
                ? RoomManager.GetCellWorldDepth()
                : 200f;

            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f);
            Gizmos.DrawWireCube(transform.position, new Vector3(width, 5f, depth));

            if (_connectors == null) return;

            foreach (var connector in _connectors)
            {
                Vector3 connectorPos = connector.PassageVisual != null
                    ? connector.PassageVisual.transform.position
                    : transform.position;

                Gizmos.color = connector.IsConnected ? Color.cyan : Color.red;
                Gizmos.DrawSphere(connectorPos, 1f);

                Gizmos.color = Color.blue;
                Vector3 direction = RoomManager.GetWorldVectorFromLocalDirection(connector.Direction, transform);
                Gizmos.DrawRay(connectorPos, direction * 5f);

                if (connector.SpawnPoint != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(connector.SpawnPoint.position, 0.8f);
                    Gizmos.DrawLine(connectorPos, connector.SpawnPoint.position);
                }
            }

            if (_centralSpawnPoint != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(_centralSpawnPoint.position, 1f);
            }
        }
    }
}