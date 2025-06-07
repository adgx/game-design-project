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
    }
}