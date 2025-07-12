using System;
using UnityEngine;

namespace Enemy.EnemyData
{
    [CreateAssetMenu(fileName = "EnemyIncognitoData", menuName = "Scriptable Objects/EnemyIncognitoData")]
    public class EnemyIncognitoData : EnemyData
    {
        public float walkPointRange = 8f;
        public float timeBetweenAttacks = 1.8f;
        public GameObject bulletPrefab;
        public float sightRange = 18f;
        public float attackRange = 16f;
        public float distanceAttackDamageMultiplier = 1f;
        public float closeAttackDamageMultiplier = 1f;
        public float longSpitAttackDamage = 20f;
		public float longSpitAttackDamageLoop1 = 20f;

		public override void SetDifficulty() {
			maxHealth = maxHealthLoop1 + (float)Math.Round(maxHealthLoop1 * difficultyMultiplier * (int)GameStatus.loopIteration);
			distanceAttackDamage = distanceAttackDamageLoop1 + (float)Math.Round(distanceAttackDamageLoop1 * difficultyMultiplier) * (int)GameStatus.loopIteration;
			closeAttackDamage = closeAttackDamageLoop1 + (float)Math.Round(closeAttackDamageLoop1 * difficultyMultiplier * (int)GameStatus.loopIteration);
            longSpitAttackDamage = longSpitAttackDamageLoop1 + (float)Math.Round(longSpitAttackDamageLoop1 * difficultyMultiplier * (int)GameStatus.loopIteration);
		}
	}
}