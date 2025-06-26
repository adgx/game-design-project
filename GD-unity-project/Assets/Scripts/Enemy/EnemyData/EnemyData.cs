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
            maxHealth += (float)Math.Round(maxHealth * difficultyMultiplier);
            distanceAttackDamage += (float)Math.Round(distanceAttackDamage * difficultyMultiplier);
			closeAttackDamage += (float)Math.Round(closeAttackDamage * difficultyMultiplier);

            // TODO: we need to evaluate if we need to change the damage taken by the enemies as well, and not only the maximum health
		}
	}
}