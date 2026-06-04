using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.Player.Application;
using BillGameCore.Modules.Player.Domain;
using BillGameCore.SharedPorts.Input;
using UnityEngine;

namespace BillGameCore.Modules.Player.Presentation
{
    public sealed class PlayerSpawner
    {
        private readonly PlayerView _prefab;
        private readonly float _moveSpeed;
        private readonly IInputCommandSource _inputCommandSource;

        public PlayerSpawner(PlayerView prefab, float moveSpeed, IInputCommandSource inputCommandSource)
        {
            _prefab = prefab;
            _moveSpeed = moveSpeed;
            _inputCommandSource = inputCommandSource;
        }

        public PlayerRuntime Spawn(Vector2 position)
        {
            
            var view = Object.Instantiate(_prefab, position, Quaternion.identity);
            var state = new PlayerState(maxHealth: 10f);
            var application = new PlayerApplication(_moveSpeed, state);

            var id = BillEntityId.New();
            var presenter = new PlayerPresenter(application, view, _inputCommandSource, id);
            view.Bind(presenter);

            var runtime = new PlayerRuntime(presenter, view, id);

            return runtime;
        }
    }
}