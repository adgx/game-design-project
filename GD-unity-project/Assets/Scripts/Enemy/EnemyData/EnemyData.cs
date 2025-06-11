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
    }
}