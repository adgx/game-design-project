using System;
using Unity.VisualScripting;
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

		// Determines the difficulty increase between different interactions
		[SerializeField] private float difficultyMultiplier = 0.33f;

		// This function sets the parameters of the Room according to the current loop iteration
		public void setDifficulty() {
			roomSpawnBudget += (int)Math.Round(roomSpawnBudget * difficultyMultiplier * (int)GameStatus.loopIteration);
        }
    }
}
