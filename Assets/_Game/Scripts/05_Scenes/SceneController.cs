using BillGameCore.Core.Rewards;
using BillGameCore.Scenes.UI;
using BillGameCore.SharedPorts.Economy;
using UnityEngine;

namespace BillGameCore.Scenes
{
    public sealed class SceneController : MonoBehaviour
    {
        private IRewardGrantService _rewardGrantService;
        private WalletHudPresenter _walletHudPresenter;
        public void SetEnemyDeathRewardFlow(
            IRewardGrantService rewardGrantService,
            WalletHudPresenter walletHudPresenter)
        {
            _rewardGrantService = rewardGrantService;
            _walletHudPresenter = walletHudPresenter;
        }
        public void HandlePlayerDied()
        {

        }

        public void HandleEnemyDied(RewardBundle reward)
        {
            _rewardGrantService?.Grant(reward);
            _walletHudPresenter?.Refresh();
        }
    }
}