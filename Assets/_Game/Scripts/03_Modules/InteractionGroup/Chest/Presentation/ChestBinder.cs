using BillGameCore.Core.Interaction;
using BillGameCore.Modules.InteractionGroup.Chest.Domain;
using BillGameCore.Modules.InteractionGroup.Chest.Infrastructure.Config;
using BillGameCore.Modules.InteractionGroup.Chest.Application;
using UnityEngine;
using System;

namespace BillGameCore.Modules.InteractionGroup.Chest.Presentation
{
    public sealed class ChestBinder : MonoBehaviour, IInteractable
    {
        private ChestPresenter _presenter;
        [SerializeField] private ChestConfig _config;

        private void Awake()
        {
            if (_config == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ChestBinder)} on '{gameObject.name}' requires a {nameof(ChestConfig)} reference.");
            }
            var definition = _config.ToDefinition();
            var state = new ChestState(definition);
            var application = new ChestApplication(state);
            var view = GetComponent<ChestView>();

            if (view == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ChestBinder)} on '{gameObject.name}' requires a {nameof(ChestView)} component.");
            }

            _presenter = new ChestPresenter(application, view);
            _presenter.Initialize();
        }

        public bool CanInteract()
        {
            return _presenter.CanInteract();
        }

        public void Interact()
        {
            _presenter.TryInteract();
        }
        
    }
}