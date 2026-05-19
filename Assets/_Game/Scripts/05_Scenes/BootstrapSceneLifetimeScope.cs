using System;
using BillGameCore.Modules.Input.Application;
using BillGameCore.Modules.Input.Commands;
using BillGameCore.Modules.Input.Infrastructure;
using BillGameCore.Modules.Player.Infrastructure.Config;
using BillGameCore.Modules.Player.Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace BillGameCore.Scenes
{
    public sealed class BootstrapSceneLifetimeScope : LifetimeScope
    {
        private const int CommandBufferCapacity = 64;

        [Header("Scene Components")]
        [SerializeField]
        private InputReader _inputReader;

        [SerializeField]
        private SceneController _sceneController;

        [Header("Player")]
        [SerializeField]
        private PlayerView _playerPrefab;

        [SerializeField]
        private PlayerConfig _playerConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            ValidateRequiredFields();

            _inputReader.ValidateConfiguration();

            builder.Register(_ => new CommandBuffer(CommandBufferCapacity), Lifetime.Scoped);
            builder.Register<InputCommandDispatcher>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.RegisterComponent(_inputReader);
            builder.RegisterComponent(_sceneController);

            builder.RegisterInstance(_playerPrefab);
            builder.RegisterInstance(_playerConfig);
            builder.Register<PlayerSpawner>(Lifetime.Scoped);

            builder.RegisterEntryPoint<SceneBootstrapper>();
        }

        private void ValidateRequiredFields()
        {
            if (_inputReader == null)
            {
                throw new InvalidOperationException($"{nameof(BootstrapSceneLifetimeScope)} requires an InputReader.");
            }

            if (_sceneController == null)
            {
                throw new InvalidOperationException($"{nameof(BootstrapSceneLifetimeScope)} requires a SceneController.");
            }

            if (_playerPrefab == null)
            {
                throw new InvalidOperationException($"{nameof(BootstrapSceneLifetimeScope)} requires a PlayerView prefab.");
            }

            if (_playerConfig == null)
            {
                throw new InvalidOperationException($"{nameof(BootstrapSceneLifetimeScope)} requires a PlayerConfig.");
            }
        }
    }
}