using BillGameCore.Core.Rewards;

namespace BillGameCore.SharedPorts.Economy
{
    // Called by SceneController after enemy dies — grants exp+gold immediately (no loot object).
    public interface IRewardGrantService
    {
        void Grant(RewardBundle bundle);
    }
}