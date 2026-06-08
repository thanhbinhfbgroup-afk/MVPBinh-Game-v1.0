using System;

namespace BillGameCore.Core.Inventory
{
    public readonly struct ItemStack
    {
        public ItemStack(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                throw new ArgumentException("Item id cannot be null or whitespace.", nameof(itemId));
            }

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Item amount must be greater than 0.");
            }

            ItemId = itemId;
            Amount = amount;
        }

        public string ItemId { get; }
        public int Amount { get; }
    }
}
