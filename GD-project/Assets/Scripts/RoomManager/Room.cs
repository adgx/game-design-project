// Room.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RoomManager // Ensure this namespace matches your other scripts
{
    // Enum to define the direction a connector faces (relative to the room itself)
    public enum ConnectorDirection { North, South, East, West }

    // Enum to categorize our room prefabs
    public enum RoomType
    {
        Room,          // A standard squarish room
        Corridor_NS,   // A corridor designed to connect North-South (long along Z-axis)
        Corridor_EW    // A corridor designed to connect East-West (long along X-axis)
    }

    // This class holds data for a single potential connection point on a room
    [System.Serializable] // Makes it visible and editable in the Inspector
    public class RoomConnector
    {
        [Tooltip("Optional identifier for this connector (e.g., 'MainNorthDoor')")]
        public string id;

        [Tooltip("The direction this connector faces outwards from the room.")]
        public ConnectorDirection direction;

        [Tooltip("The GameObject representing the OPEN passage or doorway. Should be INACTIVE in the prefab by default and positioned at the cell edge.")]
        public GameObject passageVisual; // This becomes active when connected

        [Tooltip("Transform defining where the player spawns if entering through this connector.")]
        public Transform spawnPoint;

        [HideInInspector] // This is a runtime state, not set in the prefab's Inspector
        public bool isConnected = false;

        // Gets the world position of the connector, preferring the passageVisual's actual transform.
        // Falls back to calculating based on cell edges if passageVisual is not set.
        public Vector3 GetVisualCenterWorldPosition(Transform roomTransform)
        {
            if (passageVisual != null)
            {
                // Even if inactive, its transform.position is where it *will be* when active.
                // This is crucial for Gizmos and placement logic if it relies on this.
                return passageVisual.transform.position;
            }

            // Fallback calculation: Position at the edge of the cell.
            // This is mainly for scenarios where passageVisual might not be assigned (e.g. purely conceptual connector for logic)
            // or for Gizmos when editing prefabs outside a scene with RoomManager.Instance.
            float halfWidth = (Application.isPlaying && RoomManager.Instance != null) ? RoomManager.GetCellWorldWidth() / 2f : 5f; // Default for editor
            float halfDepth = (Application.isPlaying && RoomManager.Instance != null) ? RoomManager.GetCellWorldDepth() / 2f : 5f; // Default for editor
            Vector3 localPos = Vector3.zero;
            switch (direction)
            {
                case ConnectorDirection.North: localPos = new Vector3(0, 0, halfDepth); break;
                case ConnectorDirection.South: localPos = new Vector3(0, 0, -halfDepth); break;
                case ConnectorDirection.East:  localPos = new Vector3(halfWidth, 0, 0); break;
                case ConnectorDirection.West:  localPos = new Vector3(-halfWidth, 0, 0); break;
            }
            return roomTransform.TransformPoint(localPos);
        }
    }

    public class Room : MonoBehaviour
    {
        [Header("Room Variant Properties")]
        public RoomType roomType;
        // No sizeInGridUnits needed, as each room occupies one logical grid cell.
        public List<RoomConnector> connectors = new List<RoomConnector>();
        public Transform centralSpawnPoint;

        public Vector3Int RoomIndex { get; set; } // The grid index of this single cell room

        private void Awake()
        {
            // Setup Central Spawn Point if not assigned
            if (centralSpawnPoint == null)
            {
                GameObject csPointGO = new GameObject("CentralSpawnPoint_Generated");
                csPointGO.transform.SetParent(transform);
                csPointGO.transform.localPosition = Vector3.zero; // Center of the room
                centralSpawnPoint = csPointGO.transform;
            }

            // Initialize connectors
            foreach (var connector in connectors)
            {
                if (connector.passageVisual != null)
                {
                    connector.passageVisual.SetActive(false); // Passages start INACTIVE
                }
                connector.isConnected = false;

                // Auto-generate spawn point if not set, relative to passageVisual
                if (connector.spawnPoint == null)
                {
                    GameObject spGO = new GameObject($"{connector.direction}SpawnPoint_Generated");
                    spGO.transform.SetParent(transform); // Parent to this room
                    
                    Vector3 spawnLocalPos;
                    if (connector.passageVisual != null) // Prefer positioning relative to passage visual
                    {
                        // Get the local direction that is "inward" from the passage
                        Vector3 inwardLocalDir = Vector3.zero;
                        float offsetAmount = 1.0f; // How far inside the room from the passage
                        switch(connector.direction) { // Connector.direction is the way the passage faces OUT of the room
                            case ConnectorDirection.North: inwardLocalDir = Vector3.back; break;    // Inward is South
                            case ConnectorDirection.South: inwardLocalDir = Vector3.forward; break; // Inward is North
                            case ConnectorDirection.East:  inwardLocalDir = Vector3.left; break;    // Inward is West
                            case ConnectorDirection.West:  inwardLocalDir = Vector3.right; break;   // Inward is East
                        }
                        // PassageVisual's localPosition relative to the room's pivot.
                        // Spawn point will be offset inward from this passage visual's location.
                        spawnLocalPos = connector.passageVisual.transform.localPosition + (inwardLocalDir * offsetAmount);
                    }
                    else // Fallback: position relative to cell edge if no passage visual to reference
                    {
                        float offsetFromEdge = 1.0f;
                        float cellHalfWidth = (Application.isPlaying && RoomManager.Instance != null) ? RoomManager.GetCellWorldWidth() / 2f : 5f;
                        float cellHalfDepth = (Application.isPlaying && RoomManager.Instance != null) ? RoomManager.GetCellWorldDepth() / 2f : 5f;
                        switch (connector.direction)
                        {
                            case ConnectorDirection.North: spawnLocalPos = new Vector3(0, 0, cellHalfDepth - offsetFromEdge); break;
                            case ConnectorDirection.South: spawnLocalPos = new Vector3(0, 0, -cellHalfDepth + offsetFromEdge); break;
                            case ConnectorDirection.East:  spawnLocalPos = new Vector3(cellHalfWidth - offsetFromEdge, 0, 0); break;
                            case ConnectorDirection.West:  spawnLocalPos = new Vector3(-cellHalfWidth + offsetFromEdge, 0, 0); break;
                            default: spawnLocalPos = Vector3.zero; break;
                        }
                    }
                    spGO.transform.localPosition = spawnLocalPos;
                    connector.spawnPoint = spGO.transform;
                }
            }
        }

        public RoomConnector GetConnector(ConnectorDirection dir)
        {
            return connectors.Find(c => c.direction == dir);
        }

        public List<RoomConnector> GetAvailableConnectors()
        {
            return connectors.FindAll(c => !c.isConnected);
        }

        public void ActivateConnection(ConnectorDirection directionToOpen)
        {
            RoomConnector connectorToActivate = GetConnector(directionToOpen);
            if (connectorToActivate != null)
            {
                if (connectorToActivate.passageVisual != null)
                {
                    connectorToActivate.passageVisual.SetActive(true);
                }
                connectorToActivate.isConnected = true;
            }
            // else: No connector for this direction on this room type, or already handled.
        }

        public Transform GetSpawnPointForWorldEntryDirection(Vector3Int worldEntryDirection)
        {
            // Determines which local connector side the player entered from based on world movement.
            ConnectorDirection connectorSideOfEntry = RoomManager.GetOppositeLocalDirection(worldEntryDirection);
            RoomConnector connector = GetConnector(connectorSideOfEntry);
            if (connector != null && connector.spawnPoint != null)
            {
                return connector.spawnPoint;
            }
            // Fallback to central spawn point if no specific connector/spawn point is found.
            return centralSpawnPoint;
        }

        private void OnDrawGizmosSelected()
        {
            // Use RoomManager.Instance for cell dimensions if available, otherwise use fallback values.
            float cellWidth = (Application.isPlaying && RoomManager.Instance != null) ? RoomManager.GetCellWorldWidth() : 10f;
            float cellDepth = (Application.isPlaying && RoomManager.Instance != null) ? RoomManager.GetCellWorldDepth() : 10f;

            // Draw bounds of the single grid cell this room occupies.
            // Assumes this room's transform.position is the center of that cell.
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(cellWidth, 2f, cellDepth)); // Height of 2f is arbitrary for visualization

            // Draw Gizmos for each connector
            foreach (var connector in connectors)
            {
                Vector3 connectorWorldPos = connector.GetVisualCenterWorldPosition(transform); // Get world pos of the connector visual

                // Color based on connection and visual state
                if (connector.isConnected) {
                    Gizmos.color = (connector.passageVisual != null && connector.passageVisual.activeSelf) ? Color.cyan : Color.green; // Cyan if visually open, Green if logically connected but no visual
                } else {
                    Gizmos.color = Color.red; // Not connected
                }
                Gizmos.DrawSphere(connectorWorldPos, 0.3f); // Sphere at connector visual's center

                // Draw a ray indicating the connector's outward direction
                Gizmos.color = Color.blue;
                Vector3 outwardDirWorld = RoomManager.GetWorldVectorFromLocalDirection(connector.direction, transform);
                Gizmos.DrawRay(connectorWorldPos, outwardDirWorld * 1.5f); // Line pointing out

                // Draw Gizmo for the connector's spawn point
                if (connector.spawnPoint != null)
                {
                    Gizmos.color = Color.Lerp(Color.cyan, Color.blue, 0.5f); // A mix of blue/cyan
                    Gizmos.DrawSphere(connector.spawnPoint.position, 0.2f);
                    Gizmos.DrawLine(connectorWorldPos, connector.spawnPoint.position); // Line from connector to its spawn point
                }
            }

            // Draw Gizmo for the central spawn point
            if (centralSpawnPoint != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(centralSpawnPoint.position, 0.25f);
            }
        }
    }
}