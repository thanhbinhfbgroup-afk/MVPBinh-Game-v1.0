using System;
using System.Collections.Generic;
using BillGameCore.Core.Inventory;
using BillGameCore.Core.Save;
using BillGameCore.Modules.Inventory.Domain;
using BillGameCore.Modules.Inventory.Infrastructure.Persistence;
using BillGameCore.SharedPorts.Inventory;

namespace BillGameCore.Modules.Inventory.Application
{
    // Project-scope service. Owns InventoryState and exposes only SharedPorts contracts.
    public sealed class InventoryService
        : IInventoryReadService,
          IInventoryWriteService,
          ISaveSnapshotProvider<InventorySaveData>,
          ISaveSnapshotConsumer<InventorySaveData>
    {
        private readonly InventoryState _state = new InventoryState();

        public event Action Changed;

        public IReadOnlyList<ItemStack> GetItems() => _state.Items.ToArray();

        public bool HasItem(string itemId, int minAmount = 1)
        {
            for (int i = 0; i < _state.Items.Count; i++)
            {
                var item = _state.Items[i];
                if (item.ItemId == itemId && item.Amount >= minAmount)
                    return true;
            }

            return false;
        }

        public bool AddItem(ItemStack stack)
        {
            if (string.IsNullOrWhiteSpace(stack.ItemId) || stack.Amount <= 0)
                return false;

            for (int i = 0; i < _state.Items.Count; i++)
            {
                var item = _state.Items[i];
                if (item.ItemId != stack.ItemId) continue;

                _state.Items[i] = new ItemStack(stack.ItemId, item.Amount + stack.Amount);
                Changed?.Invoke();
                return true;
            }

            _state.Items.Add(stack);
            Changed?.Invoke();
            return true;
        }

        public bool RemoveItem(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
                return false;

            for (int i = 0; i < _state.Items.Count; i++)
            {
                var item = _state.Items[i];
                if (item.ItemId != itemId) continue;
                if (item.Amount < amount) return false;

                int remaining = item.Amount - amount;
                if (remaining == 0) _state.Items.RemoveAt(i);
                else _state.Items[i] = new ItemStack(itemId, remaining);

                Changed?.Invoke();
                return true;
            }

            return false;
        }

        public InventorySaveData CreateSnapshot()
        {
            var items = new InventoryItemEntry[_state.Items.Count];
            for (int i = 0; i < _state.Items.Count; i++)
            {
                var item = _state.Items[i];
                items[i] = new InventoryItemEntry
                {
                    ItemId = item.ItemId,
                    Amount = item.Amount,
                };
            }

            return new InventorySaveData { Items = items };
        }

        public void RestoreSnapshot(InventorySaveData snapshot)
        {
            if (snapshot == null) return;

            _state.Items.Clear();
            if (snapshot.Items != null)
            {
                for (int i = 0; i < snapshot.Items.Length; i++)
                {
                    var item = snapshot.Items[i];
                    if (item == null) continue;
                    AddItem(new ItemStack(item.ItemId, item.Amount));
                }
            }

            Changed?.Invoke();
        }
    }
}
