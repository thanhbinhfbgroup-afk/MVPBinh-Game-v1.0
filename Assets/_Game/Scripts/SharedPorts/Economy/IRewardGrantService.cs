using BillGameCore.Core.Rewards;

namespace BillGameCore.SharedPorts.Economy
{
    // Gọi bởi SceneController sau khi enemy chết — cộng exp+gold ngay lập tức.
    public interface IRewardGrantService
    {
        void Grant(RewardBundle bundle);
    }
}