using System;
using UnityEngine;

namespace Enemy.EnemyData
{
    [CreateAssetMenu(fileName = "EnemyDrakeData", menuName = "Scriptable Objects/EnemyDrakeData")]
    public class EnemyDrakeData : EnemyData
    {
        public float walkPointRange = 12f;
        public float timeBetweenAttacks = 2.5f;
        public GameObject bulletPrefab;
        public float sightRange = 22f;
        public float attackRange = 18f;
        public float distanceAttackDamageMultiplier = 1f;
        public float closeAttackDamageMultiplier = 1f;

		public override void SetDifficulty() {
			maxHealth = maxHealthLoop1 + (float)Math.Round(maxHealthLoop1 * difficultyMultiplier * (int)GameStatus.loopIteration);
			distanceAttackDamage = distanceAttackDamageLoop1 + (float)Math.Round(distanceAttackDamageLoop1 * difficultyMultiplier) * (int)GameStatus.loopIteration;
			closeAttackDamage = closeAttackDamageLoop1 + (float)Math.Round(closeAttackDamageLoop1 * difficultyMultiplier * (int)GameStatus.loopIteration);
		}
	}
}