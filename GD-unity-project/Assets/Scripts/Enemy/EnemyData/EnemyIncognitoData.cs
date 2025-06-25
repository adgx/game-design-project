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
    }
}