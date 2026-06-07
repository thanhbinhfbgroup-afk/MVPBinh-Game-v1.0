using System;
using BillGameCore.Modules.InteractionGroup.ItemPickup.Application;

namespace BillGameCore.Modules.InteractionGroup.ItemPickup.Presentation
{
    public sealed class ItemPickupPresenter
    {
        private readonly ItemPickupApplication _application;
        private readonly ItemPickupView _view;

        public ItemPickupPresenter(ItemPickupApplication application, ItemPickupView view)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public bool CanInteract()
        {
            return _application.CanInteract();
        }

        public void Initialize()
        {
            _view.SetCollected(_application.IsCollected);
        }

        public ItemPickupResult TryInteract()
        {
            var result = _application.TryPickUp();
            if (!result.IsPickedUpNow)
            {
                return result;
            }

            _view.SetCollected(_application.IsCollected);
            return result;
        }
    }
}
