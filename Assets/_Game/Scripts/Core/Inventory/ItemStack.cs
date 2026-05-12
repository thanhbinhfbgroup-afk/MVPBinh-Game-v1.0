namespace BillGameCore.Core.Inventory
{
    public readonly struct ItemStack
    {
        public ItemStack(string itemId, int amount) { ItemId = itemId; Amount = amount; }
        public string ItemId { get; }
        public int    Amount { get; }
    }
}