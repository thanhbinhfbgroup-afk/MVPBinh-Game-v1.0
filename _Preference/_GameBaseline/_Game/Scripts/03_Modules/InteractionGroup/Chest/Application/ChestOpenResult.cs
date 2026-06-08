using BillGameCore.Core.Rewards;

namespace BillGameCore.Modules.InteractionGroup.Chest.Application
{
    public readonly struct ChestOpenResult
    {
        public ChestOpenResult(bool isOpenedNow, RewardBundle reward)
        {
            IsOpenedNow = isOpenedNow;
            Reward = reward;
        }

        public bool IsOpenedNow { get; }
        public RewardBundle Reward { get; }
    }
}