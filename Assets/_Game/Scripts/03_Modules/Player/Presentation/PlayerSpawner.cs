using System;
using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.Player.Application;
using BillGameCore.Modules.Player.Domain;
using BillGameCore.Modules.Player.Infrastructure.Config;
using BillGameCore.SharedPorts.Input;
using UnityEngine;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;

namespace BillGameCore.Modules.Player.Presentation
{
    public sealed class PlayerSpawner
    {
        private readonly PlayerView _prefab;
        private readonly PlayerConfig _config;
        private readonly IInputCommandSource _inputCommandSource;

        public PlayerSpawner(
            PlayerView prefab,
            PlayerConfig config,
            IInputCommandSource inputCommandSource)
        {
            _prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _inputCommandSource = inputCommandSource ?? throw new ArgumentNullException(nameof(inputCommandSource));
        }

        public PlayerRuntime Spawn(Vector2 position)
        {
            var id = EntityId.New();

            var view = UnityEngine.Object.Instantiate(_prefab, position, Quaternion.identity);
            view.name = $"Player_{id}";

            var definition = _config.ToDefinition();
            var state = new PlayerState();
            var application = new PlayerApplication(definition, state);
            var presenter = new PlayerPresenter(id, application, view, _inputCommandSource);

            view.Bind(presenter);

            return new PlayerRuntime(id, view, presenter);
        }
    }
}