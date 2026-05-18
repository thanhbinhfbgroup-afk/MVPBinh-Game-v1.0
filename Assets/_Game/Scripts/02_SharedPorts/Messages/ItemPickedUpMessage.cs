using BillGameCore.Core.Inventory;
using BillGameCore.Core.ValueObjects;

namespace BillGameCore.SharedPorts.Messages
{
    public readonly struct ItemPickedUpMessage
    {
        public ItemPickedUpMessage(EntityId pickerId, ItemStack item)
        {
            PickerId = pickerId;
            Item = item;
        }

        public EntityId PickerId { get; }
        public ItemStack Item { get; }
    }
}