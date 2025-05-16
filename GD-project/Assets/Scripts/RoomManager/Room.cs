using UnityEngine;

namespace RoomManager
{
    public class Room : MonoBehaviour
    {
        [SerializeField] private GameObject topDoor;
        [SerializeField] private GameObject bottomDoor;
        [SerializeField] private GameObject rightDoor;
        [SerializeField] private GameObject leftDoor;

        public Transform topSpawnPoint;
        public Transform bottomSpawnPoint;
        public Transform leftSpawnPoint;
        public Transform rightSpawnPoint;
        public Transform centralSpawnPoint;

        [System.Flags]
        public enum DoorFlags
        {
            None = 0,
            Right = 1 << 0,
            Left = 1 << 1,
            Bottom = 1 << 2,
            Top = 1 << 3
        }

        public DoorFlags Doors { get; set; }

        public Vector3Int RoomIndex { get; set; }

        private void Awake()
        {
            Transform spawnPointsTransform = transform.Find("SpawnPoints");

            if (spawnPointsTransform == null)
            {
                Debug.LogError("SpawnPoints GameObject is missing from the Room!");
                return;
            }

            topSpawnPoint = spawnPointsTransform.Find("TopSpawnPoint");
            if (topSpawnPoint == null) Debug.LogWarning("Top Spawn Point is missing!");

            bottomSpawnPoint = spawnPointsTransform.Find("BottomSpawnPoint");
            if (bottomSpawnPoint == null) Debug.LogWarning("Back Spawn Point is missing!");

            leftSpawnPoint = spawnPointsTransform.Find("LeftSpawnPoint");
            if (leftSpawnPoint == null) Debug.LogWarning("Left Spawn Point is missing!");

            rightSpawnPoint = spawnPointsTransform.Find("RightSpawnPoint");
            if (rightSpawnPoint == null) Debug.LogWarning("Right Spawn Point is missing!");

            centralSpawnPoint = spawnPointsTransform.Find("CentralSpawnPoint");
            if (centralSpawnPoint == null) Debug.LogWarning("Central Spawn Point is missing!");
        }


        public void OpenDoor(Vector3Int direction)
        {
            if (direction == Vector3Int.forward && topDoor != null)
            {
                topDoor.SetActive(true);
            }
            else if (direction == Vector3Int.back && bottomDoor != null)
            {
                bottomDoor.SetActive(true);
            }
            else if (direction == Vector3Int.right && rightDoor != null)
            {
                rightDoor.SetActive(true);
            }
            else if (direction == Vector3Int.left && leftDoor != null)
            {
                leftDoor.SetActive(true);
            }
        }

        public void CloseAllDoors()
        {
            if (topDoor != null)
            {
                topDoor.SetActive(false);
            }

            if (bottomDoor != null)
            {
                bottomDoor.SetActive(false);
            }

            if (rightDoor != null)
            {
                rightDoor.SetActive(false);
            }

            if (leftDoor != null)
            {
                leftDoor.SetActive(false);
            }
        }
    }
}