using BillGameCore.Core.Rewards;
using System;
using UnityEngine;

namespace BillGameCore.Modules.InteractionGroup.Chest.Infrastructure.Config
{
    [CreateAssetMenu(
        fileName = "ChestRewardConfig",
        menuName = "BillGameCore/InteractionGroup/Chest Reward Config")]
    public sealed class ChestRewardConfig : ScriptableObject
    {
        [Header("--- Shared Reward Archetype ---")]
        [SerializeField] private int _gold;
        [SerializeField] private int _experience;
        [SerializeField] private int _coin;

        public RewardBundle ToRewardBundle()
        {
            return new RewardBundle(_gold, _experience, _coin);
        }
    }
}