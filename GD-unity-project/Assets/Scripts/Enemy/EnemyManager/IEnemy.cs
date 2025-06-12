namespace Enemy.EnemyManager
{
    public interface IEnemy
    {
        void Initialize(EnemyData.EnemyData enemyData, RoomManager.RoomManager roomManager);

        void TakeDamage(int amount);
    }
}
