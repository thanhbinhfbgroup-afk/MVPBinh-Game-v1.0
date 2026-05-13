using BillGameCore.Core.Inventory;

namespace BillGameCore.SharedPorts.Messages
{
    // Publish bởi LootItemBinder sau khi nhặt item thành công (Slice 03+).
    public sealed class ItemPickedUpMessage
    {
        public ItemStack Stack { get; }
        public ItemPickedUpMessage(ItemStack stack) { Stack = stack; }
    }
}