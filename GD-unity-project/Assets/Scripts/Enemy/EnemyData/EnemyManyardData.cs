using System;
using UnityEngine;

namespace Enemy.EnemyData
{
    [CreateAssetMenu(fileName = "EnemyManyardData", menuName = "Scriptable Objects/EnemyManyardData")]
    public class EnemyManyardData : EnemyData
    {
        public float walkPointRange = 10f;
        public float timeBetweenAttacks = 2f;
        public GameObject bulletPrefab;
        public float sightRange = 20f;
        public float remoteAttackRange = 15f;
        public float closeAttackRange = 5f;
        public float bulletForce = 16f;
        public float bulletUpwardForce = 2f;
        public float distanceAttackDamageMultiplier = 1f;
        public float closeAttackDamageMultiplier = 1f;

		public override void SetDifficulty() {
			maxHealth = maxHealthLoop1 + (float)Math.Round(maxHealthLoop1 * difficultyMultiplier * (int)GameStatus.loopIteration);
			distanceAttackDamage = distanceAttackDamageLoop1 + (float)Math.Round(distanceAttackDamageLoop1 * difficultyMultiplier) * (int)GameStatus.loopIteration;
			closeAttackDamage = closeAttackDamageLoop1 + (float)Math.Round(closeAttackDamageLoop1 * difficultyMultiplier * (int)GameStatus.loopIteration);
		}
	}
}