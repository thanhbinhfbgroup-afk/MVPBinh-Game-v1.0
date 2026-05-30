using BillGameCore.Core.Rewards;

namespace BillGameCore.SharedPorts.Economy
{
    public interface IRewardGrantService
    {
        void Grant(RewardBundle bundle);
    }
}