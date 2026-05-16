namespace BillGameCore.Core.Inventory
{
    // Value object đại diện cho một lượng item cùng loại.
    // Core chỉ biết itemId + amount, không biết icon/prefab/UI/database.
    public readonly struct ItemStack : System.IEquatable<ItemStack>
    {
        public static readonly ItemStack Invalid = new ItemStack(string.Empty, 0);

        public ItemStack(string itemId, int amount)
        {
            ItemId = itemId ?? string.Empty;
            Amount = amount;
        }

        public string ItemId { get; }
        public int Amount { get; }

        public bool IsValid => !string.IsNullOrWhiteSpace(ItemId) && Amount > 0;

        public bool Equals(ItemStack other)
        {
            return ItemId == other.ItemId && Amount == other.Amount;
        }

        public override bool Equals(object obj)
        {
            return obj is ItemStack other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ItemId != null ? ItemId.GetHashCode() : 0) * 397) ^ Amount;
            }
        }

        public override string ToString()
        {
            return $"{ItemId} x{Amount}";
        }

        public static bool operator ==(ItemStack left, ItemStack right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ItemStack left, ItemStack right)
        {
            return !left.Equals(right);
        }
    }
}