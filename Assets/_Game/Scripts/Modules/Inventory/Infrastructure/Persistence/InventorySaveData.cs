namespace BillGameCore.Modules.Inventory.Infrastructure.Persistence
{
    // DTO save serializable — snapshot của InventoryState. KHÔNG phải live state object.
    [System.Serializable]
    public sealed class InventorySaveData
    {
        public InventoryItemEntry[] Items = System.Array.Empty<InventoryItemEntry>();
    }

    [System.Serializable]
    public sealed class InventoryItemEntry
    {
        public string ItemId;
        public int    Amount;
    }
}
