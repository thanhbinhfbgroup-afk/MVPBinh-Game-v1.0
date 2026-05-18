using BillGameCore.Core.Inventory;

namespace BillGameCore.SharedPorts.Inventory
{
    // Tiêu thụ bởi: LootItemBinder, ChestBinder. Implement bởi: InventoryService.
    public interface IInventoryWriteService
    {
        bool AddItem(ItemStack stack);
        bool RemoveItem(string itemId, int amount);
    }
}