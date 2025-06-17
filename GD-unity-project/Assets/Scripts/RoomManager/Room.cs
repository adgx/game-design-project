using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RoomManager
{
    public enum ConnectorDirection
    {
        North,
        South,
        East,
        West
    }

    [System.Serializable]
    public class RoomConnector
    {
        [Tooltip("An optional identifier for this connector.")] [SerializeField]
        private string _id;

        [Tooltip("The cardinal direction this connector points to.")] [SerializeField]
        private ConnectorDirection _direction;

        [Tooltip("The visual representation of the passage (e.g., a doorway model). Will be activated when connected.")]
        [SerializeField]
        private GameObject _passageVisual;

        [Tooltip("The point where the player will spawn when entering from this connector.")] [SerializeField]
        private Transform _spawnPoint;

        /// <summary>Gets the optional identifier for this connector.</summary>
        public string ID => _id;

        /// <summary>Gets the cardinal direction of this connector.</summary>
        public ConnectorDirection Direction => _direction;

        /// <summary>Gets the visual GameObject for the passage.</summary>
        public GameObject PassageVisual => _passageVisual;

        /// <summary>Gets or sets the spawn point Transform for this connector.</summary>
        public Transform SpawnPoint { get; set; }

        [HideInInspector] public bool IsConnected = false;
    }

    public class Room : MonoBehaviour, IRoom
    {
        [Header("Room Properties")]
        [Tooltip("The list of all potential connection points for this room.")]
        [SerializeField]
        private List<RoomConnector> _connectors = new List<RoomConnector>();

        [Tooltip("A central point in the room, used as a default spawn location.")] [SerializeField]
        private Transform _centralSpawnPoint;

        [Tooltip("A list of points where enemies can be spawned.")] [SerializeField]
        private List<Transform> _enemySpawnPoints;

        /// <summary>Gets the type of this room (e.g., standard room, corridor).</summary>
        public RoomType RoomType { get; private set; }

        /// <summary>Gets the maximum budget for spawning enemies in this room.</summary>
        public int MaxSpawnCost { get; private set; } = 10;

        /// <summary>Gets or sets the grid index of this room in the dungeon layout.</summary>
        public Vector3Int RoomIndex { get; set; }

        /// <summary>Gets or sets a value indicating whether enemies have been spawned in this room.</summary>
        public bool HasEnemiesSpawned { get; set; }

        /// <summary>Gets the transform for the central spawn point.</summary>
        public Transform CentralSpawnPoint => _centralSpawnPoint;

        /// <summary>Gets a read-only list of available enemy spawn points.</summary>
        public IReadOnlyList<Transform> EnemySpawnPoints => _enemySpawnPoints;

        private RoomManager _roomManager;

        /// <summary>
        /// Initializes the room with data from a ScriptableObject.
        /// </summary>
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
                GameObject csPointGO = new GameObject("CentralSpawnPoint_Generated");
                csPointGO.transform.SetParent(transform);
                csPointGO.transform.localPosition = Vector3.zero;
                _centralSpawnPoint = csPointGO.transform;
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

        private Transform CreateSpawnPointForConnector(RoomConnector connector)
        {
            GameObject spGO = new GameObject($"{connector.Direction}SpawnPoint_Generated");
            spGO.transform.SetParent(transform);

            Vector3 spawnLocalPos;
            const float offsetAmount = 1.5f;

            if (connector.PassageVisual != null)
            {
                Vector3 inwardLocalDir = -RoomManager.GetWorldVectorFromLocalDirection(connector.Direction, transform)
                    .normalized;
                spawnLocalPos = connector.PassageVisual.transform.localPosition + (inwardLocalDir * offsetAmount);
            }
            else
            {
                float cellHalfWidth = (_roomManager != null) ? RoomManager.GetCellWorldWidth() / 2f : 100f;
                float cellHalfDepth = (_roomManager != null) ? RoomManager.GetCellWorldDepth() / 2f : 100f;

                switch (connector.Direction)
                {
                    case ConnectorDirection.North:
                        spawnLocalPos = new Vector3(0, 0, cellHalfDepth - offsetAmount);
                        break;
                    case ConnectorDirection.South:
                        spawnLocalPos = new Vector3(0, 0, -cellHalfDepth + offsetAmount);
                        break;
                    case ConnectorDirection.East:
                        spawnLocalPos = new Vector3(cellHalfWidth - offsetAmount, 0, 0);
                        break;
                    case ConnectorDirection.West:
                        spawnLocalPos = new Vector3(-cellHalfWidth + offsetAmount, 0, 0);
                        break;
                    default:
                        spawnLocalPos = Vector3.zero;
                        break;
                }
            }

            spGO.transform.localPosition = spawnLocalPos;
            return spGO.transform;
        }

        /// <summary>
        /// Finds and returns the connector for a specific direction.
        /// </summary>
        /// <returns>The RoomConnector, or null if not found.</returns>
        public RoomConnector GetConnector(ConnectorDirection direction)
        {
            return _connectors.Find(c => c.Direction == direction);
        }

        /// <summary>
        /// Gets a list of all connectors that have not yet been connected to another room.
        /// </summary>
        public List<RoomConnector> GetAvailableConnectors()
        {
            return _connectors.Where(c => !c.IsConnected).ToList();
        }

        /// <summary>
        /// Activates the visual passage for a connector and marks it as connected.
        /// </summary>
        public void ActivateConnection(ConnectorDirection direction)
        {
            RoomConnector connectorToActivate = GetConnector(direction);
            if (connectorToActivate != null)
            {
                if (connectorToActivate.PassageVisual != null)
                {
                    connectorToActivate.PassageVisual.SetActive(true);
                }

                connectorToActivate.IsConnected = true;
            }
        }

        /// <summary>
        /// Gets the appropriate spawn point based on the direction the player is entering from.
        /// </summary>
        /// <param name="worldEntryDirection">The grid direction the player came from.</param>
        /// <returns>The specific spawn point for that entry, or the central spawn point as a fallback.</returns>
        public Transform GetSpawnPointForWorldEntryDirection(Vector3Int worldEntryDirection)
        {
            ConnectorDirection entrySide = RoomManager.GetOppositeLocalDirection(worldEntryDirection);
            RoomConnector connector = GetConnector(entrySide);

            if (connector != null && connector.SpawnPoint != null)
            {
                return connector.SpawnPoint;
            }

            return _centralSpawnPoint;
        }

        public void PostInitializeConnections(RoomManager roomManager)
        {
            foreach (ConnectorDirection dir in System.Enum.GetValues(typeof(ConnectorDirection)))
            {
                Vector3Int neighborIndex = this.RoomIndex + RoomManager.GetVectorFromLocalDirection(dir);

                if (roomManager.DoesRoomExistAt(neighborIndex))
                {
                    this.ActivateConnection(dir);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            float cellWidth = (Application.isPlaying && RoomManager.Instance != null)
                ? RoomManager.GetCellWorldWidth()
                : 200f;
            float cellDepth = (Application.isPlaying && RoomManager.Instance != null)
                ? RoomManager.GetCellWorldDepth()
                : 200f;

            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f);
            Gizmos.DrawWireCube(transform.position, new Vector3(cellWidth, 5f, cellDepth));

            if (_connectors == null) return;

            foreach (var connector in _connectors)
            {
                Vector3 connectorWorldPos = connector.PassageVisual != null
                    ? connector.PassageVisual.transform.position
                    : transform.position;

                Gizmos.color = connector.IsConnected ? Color.cyan : Color.red;
                Gizmos.DrawSphere(connectorWorldPos, 1f);

                Gizmos.color = Color.blue;
                Vector3 outwardDirWorld = RoomManager.GetWorldVectorFromLocalDirection(connector.Direction, transform);
                Gizmos.DrawRay(connectorWorldPos, outwardDirWorld * 5f);

                if (connector.SpawnPoint != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(connector.SpawnPoint.position, 0.8f);
                    Gizmos.DrawLine(connectorWorldPos, connector.SpawnPoint.position);
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