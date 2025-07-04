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
        public Transform SpawnPoint => _spawnPoint;

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

		[Tooltip("Prefab for a health vending machine that may spawn in the room.")] [SerializeField]
		private GameObject _healthVendingMachinePrefab;

		private float _healthVendingMachineSpawnChance;
		private bool _spawnHealthVendingMachine;

		[Tooltip("Prefab for a power up vending machine that may spawn in the room.")] [SerializeField]
        private GameObject _powerUpVendingMachinePrefab;
        
        private float _powerUpVendingMachineSpawnChance;
        private bool _spawnPowerUpVendingMachine;

        [Tooltip("Prefab for an upgrade terminal that may spawn in the room.")] [SerializeField]
        private GameObject _upgradeTerminalPrefab;
        
        private float _upgradeTerminalSpawnChance;
        private bool _spawnUpgradeTerminal;

        [Tooltip("Prefab for a collectible paper item that may appear in the room.")] [SerializeField]
        private GameObject _paperPrefab;
        
        private float _paperSpawnChance;
        private bool _spawnPaper;

        private RoomManager _roomManager;

        /// <summary>
        /// Gets the type of this room.
        /// </summary>
        public RoomType RoomType { get; private set; }

        /// <summary>
        /// Gets the maximum allowed cost for enemy spawning in this room.
        /// </summary>
        public int MaxSpawnCost { get; set; } = 10;

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
            _roomManager = roomManager;

            if (roomData == null) return;

            MaxSpawnCost = roomData.roomSpawnBudget;
            RoomType = roomData.roomType;

			_healthVendingMachineSpawnChance = roomData.healthVendingMachineSpawnChance;
			_spawnHealthVendingMachine = roomData.spawnHealthVendingMachine;

			_powerUpVendingMachineSpawnChance = roomData.powerUpVendingMachineSpawnChance;
            _spawnPowerUpVendingMachine = roomData.spawnPowerUpVendingMachine;
            
            _upgradeTerminalSpawnChance = roomData.upgradeTerminalSpawnChance;
            _spawnUpgradeTerminal = roomData.spawnUpgradeTerminal;
            
            _paperSpawnChance = roomData.paperSpawnChance;
            _spawnPaper = roomData.spawnPaper;
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
            }
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
		/// Attempts to spawn the health vending machine based on chance and eligibility.
		/// </summary>
		public bool PostInitializeHealthVendingMachine() {
			if(_healthVendingMachineSpawnChance == 0)
				return false;

			_healthVendingMachinePrefab.SetActive(_spawnHealthVendingMachine);
			return _spawnHealthVendingMachine;
		}

		/// <summary>
		/// Attempts to spawn the power up vending machine based on chance and eligibility.
		/// </summary>
		public bool PostInitializePowerUpVendingMachine()
        {
            if (_powerUpVendingMachineSpawnChance == 0) return false;

            _powerUpVendingMachinePrefab.SetActive(_spawnPowerUpVendingMachine);
            return _spawnPowerUpVendingMachine;
        }

        /// <summary>
        /// Attempts to spawn the upgrade terminal based on chance and eligibility.
        /// </summary>
        public bool PostInitializeUpgradeTerminal()
        {
            if (_upgradeTerminalSpawnChance == 0) return false;

            _upgradeTerminalPrefab.SetActive(_spawnUpgradeTerminal);
            return _spawnUpgradeTerminal;
        }

        /// <summary>
        /// Attempts to spawn a collectible paper based on chance and eligibility.
        /// </summary>
        public bool PostInitializePaper()
        {
            if (_paperSpawnChance == 0) return false;

            _paperPrefab.SetActive(_spawnPaper);
            return _spawnPaper;
        }
    }
}