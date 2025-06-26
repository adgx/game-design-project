using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Enemy.EnemyData
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Tooltip("The prefab for this enemy type.")]
        public GameObject enemyPrefab;

        [Tooltip("A friendly name for this enemy type.")]
        public string enemyName = "Basic Enemy";

        [Tooltip("How 'difficult' or 'costly' this enemy is.")]
        public int spawnCost = 1;


		public float maxHealth = 100f;
		public float baseMoveSpeed = 0f;

		public float distanceAttackDamage = 10f;
		public float closeAttackDamage = 10f;

        // Determines the difficulty increase between different interactions
        [SerializeField] private float difficultyMultiplier = 0.2f;

		public void setDifficulty() {
            maxHealth += (float)Math.Round(maxHealth * difficultyMultiplier * (int)GameStatus.loopIteration);
            distanceAttackDamage += (float)Math.Round(distanceAttackDamage * difficultyMultiplier * (int)GameStatus.loopIteration);
			closeAttackDamage += (float)Math.Round(closeAttackDamage * difficultyMultiplier * (int)GameStatus.loopIteration);
		}
	}
}