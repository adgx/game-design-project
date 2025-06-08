using UnityEngine;
using System.Collections.Generic;

namespace RoomManager
{
    public enum ConnectorDirection
    {
        North,
        South,
        East,
        West
    }

    public enum RoomType
    {
        IncubatorRoom,
        OtherRoom
    }

    [System.Serializable]
    public class RoomConnector
    {
        public string id;
        public ConnectorDirection direction;
        public GameObject passageVisual;
        public Transform spawnPoint;
        [HideInInspector] public bool isConnected = false;

        public Vector3 GetVisualCenterWorldPosition(Transform roomTransform)
        {
            if (passageVisual != null)
            {
                return passageVisual.transform.position;
            }

            float halfWidth = (Application.isPlaying && RoomManager.Instance != null)
                ? RoomManager.GetCellWorldWidth() / 2f
                : 5f;
            float halfDepth = (Application.isPlaying && RoomManager.Instance != null)
                ? RoomManager.GetCellWorldDepth() / 2f
                : 5f;
            Vector3 localPos = Vector3.zero;
            switch (direction)
            {
                case ConnectorDirection.North:
                    localPos = new Vector3(0, 0, halfDepth);
                    break;
                case ConnectorDirection.South:
                    localPos = new Vector3(0, 0, -halfDepth);
                    break;
                case ConnectorDirection.East:
                    localPos = new Vector3(halfWidth, 0, 0);
                    break;
                case ConnectorDirection.West:
                    localPos = new Vector3(-halfWidth, 0, 0);
                    break;
            }

            return roomTransform.TransformPoint(localPos);
        }
    }

    public class Room : MonoBehaviour
    {
        [Header("Room Variant Properties")] public RoomType roomType;
        public List<RoomConnector> connectors = new List<RoomConnector>();
        public Transform centralSpawnPoint;

        public Vector3Int RoomIndex { get; set; }

        private void Awake()
        {
            if (centralSpawnPoint == null)
            {
                GameObject csPointGO = new GameObject("CentralSpawnPoint_Generated");
                csPointGO.transform.SetParent(transform);
                csPointGO.transform.localPosition = Vector3.zero;
                centralSpawnPoint = csPointGO.transform;
            }

            foreach (var connector in connectors)
            {
                if (connector.passageVisual != null)
                {
                    connector.passageVisual.SetActive(false);
                }

                connector.isConnected = false;

                if (connector.spawnPoint == null)
                {
                    GameObject spGO = new GameObject($"{connector.direction}SpawnPoint_Generated");
                    spGO.transform.SetParent(transform);

                    Vector3 spawnLocalPos;
                    if (connector.passageVisual != null)
                    {
                        Vector3 inwardLocalDir = Vector3.zero;
                        float offsetAmount = 1.0f;
                        switch (connector.direction)
                        {
                            case ConnectorDirection.North:
                                inwardLocalDir = Vector3.back;
                                break;
                            case ConnectorDirection.South:
                                inwardLocalDir = Vector3.forward;
                                break;
                            case ConnectorDirection.East:
                                inwardLocalDir = Vector3.left;
                                break;
                            case ConnectorDirection.West:
                                inwardLocalDir = Vector3.right;
                                break;
                        }

                        spawnLocalPos = connector.passageVisual.transform.localPosition +
                                        (inwardLocalDir * offsetAmount);
                    }
                    else
                    {
                        float offsetFromEdge = 1.0f;
                        float cellHalfWidth = (Application.isPlaying && RoomManager.Instance != null)
                            ? RoomManager.GetCellWorldWidth() / 2f
                            : 5f;
                        float cellHalfDepth = (Application.isPlaying && RoomManager.Instance != null)
                            ? RoomManager.GetCellWorldDepth() / 2f
                            : 5f;
                        switch (connector.direction)
                        {
                            case ConnectorDirection.North:
                                spawnLocalPos = new Vector3(0, 0, cellHalfDepth - offsetFromEdge);
                                break;
                            case ConnectorDirection.South:
                                spawnLocalPos = new Vector3(0, 0, -cellHalfDepth + offsetFromEdge);
                                break;
                            case ConnectorDirection.East:
                                spawnLocalPos = new Vector3(cellHalfWidth - offsetFromEdge, 0, 0);
                                break;
                            case ConnectorDirection.West:
                                spawnLocalPos = new Vector3(-cellHalfWidth + offsetFromEdge, 0, 0);
                                break;
                            default:
                                spawnLocalPos = Vector3.zero;
                                break;
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
        }

        public Transform GetSpawnPointForWorldEntryDirection(Vector3Int worldEntryDirection)
        {
            ConnectorDirection connectorSideOfEntry = RoomManager.GetOppositeLocalDirection(worldEntryDirection);
            RoomConnector connector = GetConnector(connectorSideOfEntry);
            if (connector != null && connector.spawnPoint != null)
            {
                return connector.spawnPoint;
            }

            return centralSpawnPoint;
        }

        private void OnDrawGizmosSelected()
        {
            float cellWidth = (Application.isPlaying && RoomManager.Instance != null)
                ? RoomManager.GetCellWorldWidth()
                : 10f;
            float cellDepth = (Application.isPlaying && RoomManager.Instance != null)
                ? RoomManager.GetCellWorldDepth()
                : 10f;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(cellWidth, 2f, cellDepth));

            foreach (var connector in connectors)
            {
                Vector3 connectorWorldPos = connector.GetVisualCenterWorldPosition(transform);

                if (connector.isConnected)
                {
                    Gizmos.color = (connector.passageVisual != null && connector.passageVisual.activeSelf)
                        ? Color.cyan
                        : Color.green;
                }
                else
                {
                    Gizmos.color = Color.red;
                }

                Gizmos.DrawSphere(connectorWorldPos, 0.3f);

                Gizmos.color = Color.blue;
                Vector3 outwardDirWorld = RoomManager.GetWorldVectorFromLocalDirection(connector.direction, transform);
                Gizmos.DrawRay(connectorWorldPos, outwardDirWorld * 1.5f);

                if (connector.spawnPoint != null)
                {
                    Gizmos.color = Color.Lerp(Color.cyan, Color.blue, 0.5f);
                    Gizmos.DrawSphere(connector.spawnPoint.position, 0.2f);
                    Gizmos.DrawLine(connectorWorldPos, connector.spawnPoint.position);
                }
            }

            if (centralSpawnPoint != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(centralSpawnPoint.position, 0.25f);
            }
        }
    }
}