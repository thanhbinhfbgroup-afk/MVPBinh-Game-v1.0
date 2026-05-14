using BillGameCore.Modules.Input.Application;
using BillGameCore.Modules.Input.Commands;
using BillGameCore.Modules.Input.Infrastructure;
using BillGameCore.Modules.Enemy.Infrastructure.Config;
using BillGameCore.Modules.Enemy.Presentation;
using BillGameCore.Modules.Player.Infrastructure.Config;
using BillGameCore.Modules.Player.Presentation;
using BillGameCore.SharedPorts.Input;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace BillGameCore.Scenes
{
    // Scene-specific DI scope. Lives in Scenes to avoid Composition -> Scenes asmdef cycles.
    public sealed class BootstrapSceneLifetimeScope : LifetimeScope
    {
        [Header("Scene Components")]
        [SerializeField] private InputReader _inputReader;
        [SerializeField] private SceneController _sceneController;

        [Header("Player")]
        [SerializeField] private PlayerView _playerPrefab;
        [SerializeField] private PlayerConfig _playerConfig;

        [Header("Enemy")]
        [SerializeField] private EnemyView _enemyPrefab;
        [SerializeField] private EnemyConfig _enemyConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<CommandBuffer>(Lifetime.Scoped);
            builder.Register<InputCommandDispatcher>(Lifetime.Scoped).As<IInputCommandSource>();

            builder.RegisterComponent(_inputReader);
            builder.RegisterComponent(_sceneController);

            builder.RegisterInstance(_playerPrefab);
            builder.RegisterInstance(_playerConfig);
            builder.Register<PlayerSpawner>(Lifetime.Scoped);

            if (_enemyPrefab != null && _enemyConfig != null)
            {
                builder.RegisterInstance(_enemyPrefab);
                builder.RegisterInstance(_enemyConfig);
                builder.Register<EnemySpawner>(Lifetime.Scoped);
            }

            builder.RegisterEntryPoint<SceneBootstrapper>();
        }
    }
}
