namespace BillGameCore.Core.Inventory
{
    // DTO item + số lượng dùng chung giữa Loot, Inventory và Interaction.
    public readonly struct ItemStack
    {
        public ItemStack(string itemId, int amount) { ItemId = itemId; Amount = amount; }
        public string ItemId { get; }
        public int    Amount { get; }
    }
}