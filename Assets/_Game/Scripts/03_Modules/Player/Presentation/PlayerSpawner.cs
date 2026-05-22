using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.Player.Application;
using BillGameCore.SharedPorts.Input;
using UnityEngine;

namespace BillGameCore.Modules.Player.Presentation
{
    public sealed class PlayerSpawner
    {
        private readonly PlayerView _prefab;
        private readonly float _moveSpeed;

        public PlayerSpawner(PlayerView prefab, float moveSpeed)
        {
            _prefab = prefab;
            _moveSpeed = moveSpeed;
        }

        public PlayerRuntime Spawn(Vector2 position, IInputCommandSource inputCommandSource)
        {
            
            var view = Object.Instantiate(_prefab, position, Quaternion.identity);
            var application = new PlayerApplication(_moveSpeed);

            var id = BillEntityId.New();
            var presenter = new PlayerPresenter(application, view, inputCommandSource, id);

            var runtime = new PlayerRuntime(presenter,id);

            return runtime;
        }
    }
}