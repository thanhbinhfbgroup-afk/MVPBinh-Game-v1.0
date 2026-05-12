using BillGameCore.Core.Inventory;

namespace BillGameCore.SharedPorts.Messages
{
    // Published by LootItemBinder after a successful pickup (Slice 03+).
    public sealed class ItemPickedUpMessage
    {
        public ItemStack Stack { get; }
        public ItemPickedUpMessage(ItemStack stack) { Stack = stack; }
    }
}