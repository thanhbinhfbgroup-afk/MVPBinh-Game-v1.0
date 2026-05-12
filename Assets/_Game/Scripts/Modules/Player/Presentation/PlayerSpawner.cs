using EntityId = BillGameCore.Core.ValueObjects.EntityId;
using BillGameCore.Modules.Player.Application;
using BillGameCore.Modules.Player.Domain;
using BillGameCore.Modules.Player.Infrastructure;
using BillGameCore.SharedPorts.Input;
using UnityEngine;

namespace BillGameCore.Modules.Player.Presentation
{
    // Registered in SceneLifetimeScope.
    // Receives prefab, config, and IInputCommandSource via DI constructor injection.
    // Creates per-entity Runtime — NEVER registers it back into DI (R04).
    // R18: EntityId.New() is called here — the ONLY valid location for this entity type.
    public sealed class PlayerSpawner
    {
        private readonly PlayerView          _prefab;
        private readonly PlayerConfig        _config;
        private readonly IInputCommandSource _inputSource;

        public PlayerSpawner(PlayerView prefab, PlayerConfig config, IInputCommandSource inputSource)
        {
            _prefab      = prefab;
            _config      = config;
            _inputSource = inputSource;
        }

        public PlayerRuntime Spawn(Vector2 position)
        {
            var id   = EntityId.New();                 // R18
            var def  = _config.ToDefinition();
            var state= new PlayerState();
            var app  = new PlayerApplication(id, def, state);

            // R17: Object.Instantiate is correct here because PlayerView has no [Inject] fields.
            // If you add [Inject] to PlayerView later, switch to container.Instantiate().
            var view      = Object.Instantiate(_prefab, position, Quaternion.identity);
            var presenter = new PlayerPresenter(app, view, _inputSource);
            view.Bind(presenter);

            return new PlayerRuntime(id, def, state, app, view, presenter);
        }
    }
}