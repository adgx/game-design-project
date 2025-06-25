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
    }
}