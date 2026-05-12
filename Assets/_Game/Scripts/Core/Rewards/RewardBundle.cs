using BillGameCore.Core.Inventory;

namespace BillGameCore.Core.Rewards
{
    // Emitted by EnemyApplication.OnDied.
    // Consumed by SceneController → LootSpawner + IRewardGrantService.
    public sealed class RewardBundle
    {
        public int         Gold       = 0;
        public int         Experience = 0;
        public ItemStack[] Items      = System.Array.Empty<ItemStack>();
    }
}