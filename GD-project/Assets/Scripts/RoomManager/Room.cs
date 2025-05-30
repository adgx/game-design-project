using UnityEngine;

namespace RoomManager
{
    public class Room : MonoBehaviour
    {
        [Header("Door GameObjects")]
        [SerializeField] private GameObject topDoor;
        [SerializeField] private GameObject bottomDoor;
        [SerializeField] private GameObject rightDoor;
        [SerializeField] private GameObject leftDoor;

        [Header("Spawn Points")]
        public Transform topSpawnPoint;
        public Transform bottomSpawnPoint;
        public Transform leftSpawnPoint;
        public Transform rightSpawnPoint;
        public Transform centralSpawnPoint;

        [System.Flags]
        public enum DoorFlags
        {
            None   = 0,
            Right  = 1 << 0,
            Left   = 1 << 1,
            Bottom = 1 << 2,
            Top    = 1 << 3
        }

        public DoorFlags Doors { get; set; }
        public Vector3Int RoomIndex { get; set; }

        private void Awake()
        {
            var spawnPointsContainer = transform.Find("SpawnPoints");
            if (spawnPointsContainer)
            {
                topSpawnPoint = spawnPointsContainer.Find("TopSpawnPoint");
                bottomSpawnPoint = spawnPointsContainer.Find("BottomSpawnPoint");
                leftSpawnPoint = spawnPointsContainer.Find("LeftSpawnPoint");
                rightSpawnPoint = spawnPointsContainer.Find("RightSpawnPoint");
                centralSpawnPoint = spawnPointsContainer.Find("CentralSpawnPoint");
            }
            else
            {
                Debug.LogWarning($"Room '{name}' is missing 'SpawnPoints' child GameObject for organized spawn points.", this);
                if(!centralSpawnPoint) centralSpawnPoint = transform;
            }
            
            if (!centralSpawnPoint) Debug.LogError($"Room '{name}': CentralSpawnPoint is not assigned and not found!", this);

            CloseAllDoors();
        }
        
        public void OpenDoor(Vector3Int direction)
        {
            if (direction == Vector3Int.forward && topDoor) topDoor.SetActive(true);
            else if (direction == Vector3Int.back && bottomDoor) bottomDoor.SetActive(true);
            else if (direction == Vector3Int.right && rightDoor) rightDoor.SetActive(true);
            else if (direction == Vector3Int.left && leftDoor) leftDoor.SetActive(true);
        }

        public void CloseAllDoors()
        {
            if (topDoor) topDoor.SetActive(false);
            if (bottomDoor) bottomDoor.SetActive(false);
            if (rightDoor) rightDoor.SetActive(false);
            if (leftDoor) leftDoor.SetActive(false);
        }
    }
}