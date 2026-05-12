using BillGameCore.Core.Inventory;
using System.Collections.Generic;

namespace BillGameCore.SharedPorts.Inventory
{
    public interface IInventoryReadService
    {
        IReadOnlyList<ItemStack> GetItems();
        bool HasItem(string itemId, int minAmount = 1);
    }
}