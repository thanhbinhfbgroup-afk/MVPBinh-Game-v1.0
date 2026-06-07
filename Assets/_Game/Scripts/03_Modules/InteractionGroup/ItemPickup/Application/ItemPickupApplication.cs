using BillGameCore.Core.Inventory;
using BillGameCore.Modules.InteractionGroup.ItemPickup.Domain;

namespace BillGameCore.Modules.InteractionGroup.ItemPickup.Application
{
    public sealed class ItemPickupApplication
    {
        private readonly ItemStack _itemStack;
        private readonly ItemPickupState _state;

        public ItemPickupApplication(ItemStack itemStack, ItemPickupState state)
        {
            _itemStack = itemStack;
            _state = state;
        }

        public bool IsCollected => _state.IsCollected;

        public bool CanInteract()
        {
            return !_state.IsCollected;
        }

        public ItemPickupResult TryPickUp()
        {
            if (_state.IsCollected)
            {
                return new ItemPickupResult(false, default);
            }

            _state.MarkCollected();
            return new ItemPickupResult(true, _itemStack);
        }
    }

    public readonly struct ItemPickupResult
    {
        public ItemPickupResult(bool isPickedUpNow, ItemStack itemStack)
        {
            IsPickedUpNow = isPickedUpNow;
            ItemStack = itemStack;
        }

        public bool IsPickedUpNow { get; }
        public ItemStack ItemStack { get; }
    }
}
