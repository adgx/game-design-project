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
		public float maxHealthLoop1 = 100f;

		public float baseMoveSpeed = 5f;

		public float distanceAttackDamage = 10f;
		public float distanceAttackDamageLoop1 = 10f;

		public float closeAttackDamage = 10f;
		public float closeAttackDamageLoop1 = 10f;

		// Determines the difficulty increase between different interactions
		[SerializeField] private float difficultyMultiplier = 0.2f;

		public void setDifficulty() {
			maxHealth = maxHealthLoop1 + (float)Math.Round(maxHealthLoop1 * difficultyMultiplier * (int)GameStatus.loopIteration);
			distanceAttackDamage = distanceAttackDamageLoop1 + (float)Math.Round(distanceAttackDamageLoop1 * difficultyMultiplier) * (int)GameStatus.loopIteration;
			closeAttackDamage = closeAttackDamageLoop1 + (float)Math.Round(closeAttackDamageLoop1 * difficultyMultiplier * (int)GameStatus.loopIteration);
		}
	}
}