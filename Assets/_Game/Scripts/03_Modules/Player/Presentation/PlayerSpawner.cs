using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.Player.Application;
using BillGameCore.Modules.Player.Domain;
using BillGameCore.SharedPorts.Input;
using BillGameCore.Modules.Player.Infrastructure.Config;
using UnityEngine;

namespace BillGameCore.Modules.Player.Presentation
{
    public sealed class PlayerSpawner
    {
        private readonly PlayerConfig _config;
        private readonly PlayerView _prefab;
        private readonly IInputCommandSource _inputCommandSource;

        public PlayerSpawner(PlayerView prefab, PlayerConfig config, IInputCommandSource inputCommandSource)
        {
            _prefab = prefab;
            _config = config;
            _inputCommandSource = inputCommandSource;
        }

        public PlayerRuntime Spawn(Vector2 position)
        {

            var view = Object.Instantiate(_prefab, position, Quaternion.identity);
            view.Configure(_config);

            var state = new PlayerState(maxHealth: _config.MaxHealth);
            var application = new PlayerApplication(_config.MoveSpeed, state);

            var id = BillEntityId.New();
            var presenter = new PlayerPresenter(application, view, _inputCommandSource, id, _config.AttackDamage);
            view.Bind(presenter);

            var runtime = new PlayerRuntime(presenter, view, id);

            return runtime;
        }
    }
}