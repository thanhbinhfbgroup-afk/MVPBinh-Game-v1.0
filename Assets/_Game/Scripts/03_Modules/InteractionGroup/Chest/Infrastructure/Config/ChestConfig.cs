using BillGameCore.Core.Rewards;
using BillGameCore.Modules.InteractionGroup.Chest.Domain;
using UnityEngine;
using System;

namespace BillGameCore.Modules.InteractionGroup.Chest.Infrastructure.Config
{
    public sealed class ChestConfig : MonoBehaviour
    {
        [SerializeField] private bool _startsOpened;
        [SerializeField] private ChestRewardConfig _rewardConfig;

        public ChestDefinition ToDefinition()
        {
            if (_rewardConfig == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ChestConfig)} on '{gameObject.name}' requires a {nameof(ChestRewardConfig)} reference.");
            }

            var reward = _rewardConfig.ToRewardBundle();
            return new ChestDefinition(_startsOpened, reward);
        }
    }
}