using BillGameCore.Core.Rewards;

namespace BillGameCore.SharedPorts.Economy
{
    // Contract grant RewardBundle vào các hệ thống economy/progression/inventory.
    // Có thể được gọi bởi scene flow sau enemy death, chest, quest hoặc loot pickup.
    public interface IRewardGrantService
    {
        void Grant(RewardBundle bundle);
    }
}