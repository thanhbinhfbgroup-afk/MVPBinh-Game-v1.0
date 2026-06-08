using BillGameCore.SharedPorts.Inventory;

namespace BillGameCore.Scenes.UI
{
    public sealed class InventoryHudPresenter
    {
        private readonly IInventoryReadService _inventoryReadService;
        private readonly InventoryHudView _view;
        private readonly string _itemId;
        private readonly string _itemLabel;

        public InventoryHudPresenter(
            IInventoryReadService inventoryReadService,
            InventoryHudView view,
            string itemId,
            string itemLabel)
        {
            _inventoryReadService = inventoryReadService;
            _view = view;
            _itemId = itemId;
            _itemLabel = itemLabel;
        }

        public void Refresh()
        {
            _view.SetItemCount(_itemLabel, _inventoryReadService.GetAmount(_itemId));
        }
    }
}
