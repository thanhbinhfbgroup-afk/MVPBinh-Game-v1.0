using BillGameCore.Modules.Economy.Application;
using BillGameCore.Modules.Input.Application;
using BillGameCore.Modules.Input.Commands;
using BillGameCore.Modules.Input.Infrastructure;
using BillGameCore.Modules.Player.Infrastructure.Config;
using BillGameCore.Modules.Player.Presentation;
using BillGameCore.Scenes.UI;
using BillGameCore.SharedPorts.Economy;
using BillGameCore.SharedPorts.Input;
using BillGameCore.Modules.Enemy.Infrastructure.Config;
using BillGameCore.Modules.Enemy.Presentation;
using BillGameCore.Modules.InteractionGroup.Chest.Presentation;
using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace BillGameCore.Scenes
{
    public sealed class BootstrapSceneLifetimeScope : LifetimeScope
    {
        [SerializeField] private InputReader _inputReader;
        [SerializeField] private SceneController _sceneController;
        [SerializeField] private PlayerView _playerPrefab;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private SceneBootstrapper _sceneBootstrapper;
        [SerializeField] private WalletHudView _walletHudView;
        [SerializeField] private EnemyConfig _enemyConfig;
        [SerializeField] private ChestBinder[] _chests;

        protected override void Configure(IContainerBuilder builder)
        {
            ValidateRequiredReference(_inputReader, nameof(_inputReader));
            ValidateRequiredReference(_sceneController, nameof(_sceneController));
            ValidateRequiredReference(_playerPrefab, nameof(_playerPrefab));
            ValidateRequiredReference(_playerConfig, nameof(_playerConfig));
            ValidateRequiredReference(_sceneBootstrapper, nameof(_sceneBootstrapper));
            ValidateRequiredReference(_walletHudView, nameof(_walletHudView));
            ValidateRequiredReference(_enemyConfig, nameof(_enemyConfig));
            if (_chests == null)
            {
                throw new InvalidOperationException(
                    $"BootstrapSceneLifetimeScope requires '{nameof(_chests)}' to be assigned in the Inspector.");
            }

            builder.RegisterComponent(_inputReader);
            builder.RegisterComponent(_sceneController);
            builder.RegisterComponent(_sceneBootstrapper);
            builder.RegisterComponent(_walletHudView);
            for (var i = 0; i < _chests.Length; i++)
            {
                var chest = _chests[i];
                if (chest == null)
                {
                    continue;
                }

                builder.RegisterComponent(chest);
            }
            builder.RegisterInstance(_enemyConfig);
            builder.RegisterInstance(_playerPrefab);
            builder.RegisterInstance(_playerConfig);

            builder.Register(_ => new CommandBuffer(), Lifetime.Scoped);
            builder.Register<InputCommandDispatcher>(Lifetime.Scoped).As<IInputCommandSource>();
            builder.Register<PlayerSpawner>(Lifetime.Scoped);
            builder.Register<RewardGrantService>(Lifetime.Scoped).AsSelf().As<IRewardGrantService>().As<IWalletService>();
            builder.Register<WalletHudPresenter>(Lifetime.Scoped);
            builder.Register<EnemySpawner>(Lifetime.Scoped);
        }

        private static void ValidateRequiredReference(UnityEngine.Object reference, string fieldName)
        {
            if (reference == null)
            {
                throw new InvalidOperationException(
                    $"BootstrapSceneLifetimeScope requires '{fieldName}' to be assigned in the Inspector.");
            }
        }
    }
}