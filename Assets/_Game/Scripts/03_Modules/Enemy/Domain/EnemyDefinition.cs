using BillGameCore.Core.Rewards;

namespace BillGameCore.Modules.Enemy.Domain
{
    public sealed class EnemyDefinition
    {
        public EnemyDefinition(float maxHealth, float moveSpeed, float stopDistance, float contactDamage, float contactDamageInterval, RewardBundle reward)
        {
            MaxHealth = maxHealth;
            MoveSpeed = moveSpeed;
            StopDistance = stopDistance;
            ContactDamage = contactDamage;
            ContactDamageInterval = contactDamageInterval;
            Reward = reward;
        }

        public float MaxHealth { get; }
        public float MoveSpeed { get; }
        public float StopDistance { get; }
        public float ContactDamage { get; }
        public float ContactDamageInterval { get; }
        public RewardBundle Reward { get; }
    }
}