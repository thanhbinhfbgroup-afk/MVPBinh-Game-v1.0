using System;
using BillGameCore.Modules.Input.Infrastructure;
using BillGameCore.Modules.Player.Presentation;
using UnityEngine;
using VContainer.Unity;

namespace BillGameCore.Scenes
{
    public sealed class SceneBootstrapper : IStartable, IDisposable
    {
        private readonly PlayerSpawner _playerSpawner;
        private readonly InputReader _inputReader;

        private PlayerRuntime _playerRuntime;

        public SceneBootstrapper(
            PlayerSpawner playerSpawner,
            InputReader inputReader)
        {
            _playerSpawner = playerSpawner ?? throw new ArgumentNullException(nameof(playerSpawner));
            _inputReader = inputReader ?? throw new ArgumentNullException(nameof(inputReader));
        }

        public void Start()
        {
            _playerRuntime = _playerSpawner.Spawn(Vector2.zero);
            _inputReader.SetControlledEntity(_playerRuntime.Id);
        }

        public void Dispose()
        {
            _playerRuntime?.Dispose();
            _playerRuntime = null;
        }
    }
}