using BillGameCore.Core.Rewards;
using BillGameCore.SharedPorts.Economy;

namespace BillGameCore.Modules.Economy.Application
{
    public sealed class RewardGrantService : IRewardGrantService, IWalletService
    {
        public int Gold { get; private set; }
        public int Experience { get; private set; }
        public int Coin { get; private set; }

        public void Grant(RewardBundle bundle)
        {
            Gold += bundle.Gold;
            Experience += bundle.Experience;
            Coin += bundle.Coin;
        }
    }
}