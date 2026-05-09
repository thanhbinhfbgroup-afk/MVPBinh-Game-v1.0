using BillGameCore.Interfaces.Signals;
using BillGameCore.Modules.Input.Providers;
using BillGameCore.Modules.Player;
using BillGameCore.Modules.Player.Spawners;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class SceneLifetimeScope : LifetimeScope
{
    [Header("Player Spawn")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Vector3 _spawnPosition = Vector3.zero;

    protected override void Configure(IContainerBuilder builder)
    {
        Debug.Log($"[CHECK] Scope {gameObject.name} is running!");

        var options = builder.RegisterMessagePipe();

        // Signals
        builder.RegisterMessageBroker<OnMoveInputSignal>(options);
        builder.RegisterMessageBroker<AttackInputSignal>(options);
        builder.RegisterMessageBroker<PlayerStateChangedSignal>(options);

        // Player module
        builder.Register<MovementLogic>(Lifetime.Scoped).AsImplementedInterfaces().AsSelf();
        builder.Register<PlayerSpawner>(Lifetime.Scoped);

        // Data for EntryPoint
        builder.RegisterInstance(_playerPrefab);
        builder.RegisterInstance(_spawnPosition);

        // Runtime spawn trigger
        builder.RegisterEntryPoint<PlayerSpawnEntryPoint>();
    }
}



