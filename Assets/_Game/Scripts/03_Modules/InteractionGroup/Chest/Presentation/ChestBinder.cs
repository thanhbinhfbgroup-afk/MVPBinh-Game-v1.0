using BillGameCore.Core.Interaction;
using BillGameCore.Modules.InteractionGroup.Chest.Domain;
using BillGameCore.Modules.InteractionGroup.Chest.Infrastructure.Config;
using BillGameCore.Modules.InteractionGroup.Chest.Application;
using BillGameCore.SharedPorts.Economy;
using UnityEngine;
using System;

namespace BillGameCore.Modules.InteractionGroup.Chest.Presentation
{
    public sealed class ChestBinder : MonoBehaviour, IInteractable
    {
        private ChestPresenter _presenter;
        [SerializeField] private ChestConfig _config;
        private IRewardGrantService _rewardGrantService;

        private void Awake()
        {
            if (_config == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ChestBinder)} on '{gameObject.name}' requires a {nameof(ChestConfig)} reference.");
            }
            var definition = _config.ToDefinition();
            var state = new ChestState(definition);
            var application = new ChestApplication(definition, state);
            var view = GetComponent<ChestView>();

            if (view == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ChestBinder)} on '{gameObject.name}' requires a {nameof(ChestView)} component.");
            }

            _presenter = new ChestPresenter(application, view);
            _presenter.Initialize();
        }

        public void SetRewardGrantService(IRewardGrantService rewardGrantService)
        {
            _rewardGrantService = rewardGrantService;
        }

        public bool CanInteract()
        {
            return _presenter.CanInteract();
        }

        public void Interact()
        {
            var result = _presenter.TryInteract();
            if (!result.IsOpenedNow)
            {
                return;
            }
            if (!result.Reward.IsEmpty)
            {
                _rewardGrantService?.Grant(result.Reward);
            }
            Debug.Log(
                $"Chest opened. Reward: Gold={result.Reward.Gold}, Experience={result.Reward.Experience}.",
                this);
        }
        
    }
}