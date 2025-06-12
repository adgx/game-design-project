using UnityEngine;

namespace RoomManager.RoomData
{
    [CreateAssetMenu(fileName = "RoomData", menuName = "Scriptable Objects/RoomData")]
    public class RoomData : ScriptableObject
    {
        public GameObject roomPrefab;

        public string roomName;

        public int roomSpawnBudget;

        public RoomType roomType;
    }
}
