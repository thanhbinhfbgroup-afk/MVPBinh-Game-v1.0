using BillGameCore.Core.Rewards;
using BillGameCore.SharedPorts.Economy;
using UnityEngine;

namespace BillGameCore.Scenes
{
    public sealed class SceneController : MonoBehaviour
    {
        private IRewardGrantService _rewardGrantService;

        public void SetRewardGrantService(IRewardGrantService rewardGrantService)
        {
            _rewardGrantService = rewardGrantService;
        }

        public void HandleEnemyDied(RewardBundle reward)
        {
            _rewardGrantService?.Grant(reward);
        }

        public void HandlePlayerDied()
        {
            Debug.Log("Player died.", this);
        }
    }
}
