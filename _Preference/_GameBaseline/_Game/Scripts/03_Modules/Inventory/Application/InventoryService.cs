using System;
using BillGameCore.Core.Inventory;
using BillGameCore.Modules.Inventory.Domain;
using BillGameCore.SharedPorts.Inventory;

namespace BillGameCore.Modules.Inventory.Application
{
    public sealed class InventoryService : IInventoryReadService, IInventoryWriteService
    {
        private readonly InventoryState _state = new();

        public int GetAmount(string itemId)
        {
            return _state.GetAmount(itemId);
        }

        public void AddItem(ItemStack itemStack)
        {
            if (string.IsNullOrWhiteSpace(itemStack.ItemId))
            {
                throw new ArgumentException("Item id cannot be null or whitespace.", nameof(itemStack));
            }

            _state.Add(itemStack.ItemId, itemStack.Amount);
        }
    }
}
