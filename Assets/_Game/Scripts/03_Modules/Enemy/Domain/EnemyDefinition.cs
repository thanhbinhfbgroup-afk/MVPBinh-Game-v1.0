using BillGameCore.Core.Rewards;

namespace BillGameCore.Modules.Enemy.Domain
{
    public sealed class EnemyDefinition
    {
        public EnemyDefinition(float maxHealth, RewardBundle reward)
        {
            MaxHealth = maxHealth;
            Reward = reward;
        }

        public float MaxHealth { get; }
        public RewardBundle Reward { get; }
    }
}