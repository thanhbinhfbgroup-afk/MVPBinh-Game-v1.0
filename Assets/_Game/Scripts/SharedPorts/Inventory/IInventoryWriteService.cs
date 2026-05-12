using BillGameCore.Core.Inventory;

namespace BillGameCore.SharedPorts.Inventory
{
    public interface IInventoryWriteService
    {
        bool AddItem(ItemStack stack);
        bool RemoveItem(string itemId, int amount);
    }
}