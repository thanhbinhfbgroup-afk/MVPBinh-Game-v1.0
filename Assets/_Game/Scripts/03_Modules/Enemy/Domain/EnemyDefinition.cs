using BillGameCore.Core.Rewards;

namespace BillGameCore.Modules.Enemy.Domain
{
    public sealed class EnemyDefinition
    {
        public EnemyDefinition(RewardBundle reward)
        {
            Reward = reward;
        }

        public RewardBundle Reward { get; }
    }
}