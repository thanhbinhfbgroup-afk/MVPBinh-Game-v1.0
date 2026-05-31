using BillGameCore.Core.Rewards;

namespace BillGameCore.Modules.Enemy.Application
{
    public readonly struct EnemyDeathResult
    {
        public EnemyDeathResult(bool justDied, RewardBundle reward)
        {
            JustDied = justDied;
            Reward = reward;
        }

        public bool JustDied { get; }

        public RewardBundle Reward { get; }
    }
}