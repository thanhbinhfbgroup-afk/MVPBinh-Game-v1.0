using System;
using System.Collections.Generic;

namespace BillGameCore.Modules.Inventory.Domain
{
    public sealed class InventoryState
    {
        private readonly Dictionary<string, int> _itemAmounts = new(StringComparer.Ordinal);

        public int GetAmount(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return 0;
            }

            return _itemAmounts.TryGetValue(itemId, out var amount) ? amount : 0;
        }

        public void Add(string itemId, int amount)
        {
            if (_itemAmounts.TryGetValue(itemId, out var currentAmount))
            {
                _itemAmounts[itemId] = currentAmount + amount;
                return;
            }

            _itemAmounts[itemId] = amount;
        }
    }
}
