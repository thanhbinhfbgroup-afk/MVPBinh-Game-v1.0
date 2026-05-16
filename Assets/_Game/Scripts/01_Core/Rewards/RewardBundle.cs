using System;
using System.Collections.Generic;
using BillGameCore.Core.Inventory;

namespace BillGameCore.Core.Rewards
{
    // DTO phần thưởng dùng chung cho enemy death, chest, quest, economy và loot flow.
    public readonly struct RewardBundle
    {
        public RewardBundle(
            IReadOnlyList<ItemStack> items,
            int gold = 0,
            int experience = 0)
        {
            Items = items ?? Array.Empty<ItemStack>();
            Gold = Math.Max(0, gold);
            Experience = Math.Max(0, experience);
        }

        public IReadOnlyList<ItemStack> Items { get; }
        public int Gold { get; }
        public int Experience { get; }

        public bool IsEmpty => Items.Count == 0 && Gold == 0 && Experience == 0;

        public static RewardBundle Empty => new RewardBundle(Array.Empty<ItemStack>());
    }
}