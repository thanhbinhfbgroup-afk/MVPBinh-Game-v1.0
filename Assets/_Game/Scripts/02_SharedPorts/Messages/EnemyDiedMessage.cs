using BillGameCore.Core.Rewards;
using BillGameCore.Core.ValueObjects;

namespace BillGameCore.SharedPorts.Messages
{
    public readonly struct EnemyDiedMessage
    {
        public EnemyDiedMessage(EntityId enemyId, RewardBundle reward)
        {
            EnemyId = enemyId;
            Reward = reward;
        }

        public EntityId EnemyId { get; }
        public RewardBundle Reward { get; }
    }
}