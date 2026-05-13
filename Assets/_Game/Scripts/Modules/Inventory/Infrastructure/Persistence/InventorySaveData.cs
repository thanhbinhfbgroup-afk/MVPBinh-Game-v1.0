namespace BillGameCore.Modules.Inventory.Infrastructure.Persistence
{
    // DTO save serializable — snapshot của InventoryState. KHÔNG phải live state object.
    [System.Serializable]
    public sealed class InventorySaveData
    {
        // Mirror các field của InventoryState sang kiểu serializable.
    }
}