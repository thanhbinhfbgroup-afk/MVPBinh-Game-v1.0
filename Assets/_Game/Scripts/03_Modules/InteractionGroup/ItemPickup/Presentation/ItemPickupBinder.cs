using System;
using BillGameCore.Core.Interaction;
using BillGameCore.Core.Inventory;
using BillGameCore.Modules.InteractionGroup.ItemPickup.Application;
using BillGameCore.Modules.InteractionGroup.ItemPickup.Domain;
using BillGameCore.SharedPorts.Inventory;
using UnityEngine;
using VContainer;

namespace BillGameCore.Modules.InteractionGroup.ItemPickup.Presentation
{
    public sealed class ItemPickupBinder : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _itemId = "wood";
        [SerializeField] private int _amount = 1;

        private IInventoryWriteService _inventoryWriteService;
        private Action _pickedUpCallback;
        private ItemPickupPresenter _presenter;
        private bool _isInitialized;

        [Inject]
        public void Construct(IInventoryWriteService inventoryWriteService)
        {
            _inventoryWriteService = inventoryWriteService;
        }

        public void Configure(string itemId, int amount)
        {
            _itemId = itemId;
            _amount = amount;
            TryInitializeRuntimeGraph();
        }

        private void Awake()
        {
            TryInitializeRuntimeGraph();
        }

        public void SetPickedUpCallback(Action pickedUpCallback)
        {
            _pickedUpCallback = pickedUpCallback;
        }

        public bool CanInteract()
        {
            EnsurePresenter();
            return _presenter.CanInteract();
        }

        public void Interact()
        {
            EnsurePresenter();

            if (_inventoryWriteService == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ItemPickupBinder)} on '{gameObject.name}' requires {nameof(IInventoryWriteService)} before Interact().");
            }

            var result = _presenter.TryInteract();
            if (!result.IsPickedUpNow)
            {
                return;
            }

            _inventoryWriteService.AddItem(result.ItemStack);
            _pickedUpCallback?.Invoke();

            Debug.Log(
                $"Picked item '{result.ItemStack.ItemId}' x{result.ItemStack.Amount}.",
                this);
        }

        private void TryInitializeRuntimeGraph()
        {
            if (_isInitialized)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_itemId) || _amount <= 0)
            {
                return;
            }

            var view = GetComponent<ItemPickupView>();
            if (view == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ItemPickupBinder)} on '{gameObject.name}' requires a {nameof(ItemPickupView)} component.");
            }

            var itemStack = new ItemStack(_itemId, _amount);
            var state = new ItemPickupState();
            var application = new ItemPickupApplication(itemStack, state);
            _presenter = new ItemPickupPresenter(application, view);
            _presenter.Initialize();
            _isInitialized = true;
        }

        private void EnsurePresenter()
        {
            TryInitializeRuntimeGraph();

            if (_presenter == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ItemPickupBinder)} on '{gameObject.name}' is not initialized.");
            }
        }
    }
}
