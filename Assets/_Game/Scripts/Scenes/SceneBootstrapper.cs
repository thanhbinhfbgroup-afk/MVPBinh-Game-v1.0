using System;
using BillGameCore.Modules.Input.Infrastructure;
using BillGameCore.Modules.Player.Presentation;
using UnityEngine;
using VContainer.Unity;

namespace BillGameCore.Scenes
{
    // Scene startup orchestration. Keeps scene-specific wiring out of Composition.
    public sealed class SceneBootstrapper : IStartable, IDisposable
    {
        private readonly PlayerSpawner   _playerSpawner;
        private readonly InputReader     _inputReader;
        private readonly SceneController _sceneController;
        private readonly Transform _playerSpawnpoint;

        private PlayerRuntime _playerRuntime;

        public SceneBootstrapper(PlayerSpawner playerSpawner,
                                 InputReader inputReader,
                                 SceneController sceneController,
                                 Transform playerSpawnpoint
                                
            )
        {
            _playerSpawner   = playerSpawner;
            _inputReader     = inputReader;
            _sceneController = sceneController;
           _playerSpawnpoint = playerSpawnpoint;
        }

        public void Start()
        {
            _playerRuntime = _playerSpawner.Spawn(_playerSpawnpoint.position);
            _inputReader.SetControlledEntity(_playerRuntime.Id);
            _playerRuntime.Presenter.OnDiedCallback = _sceneController.HandlePlayerDied;
        }

        public void Dispose()
        {
            if (_playerRuntime == null) return;

            _playerRuntime.Presenter.OnDiedCallback -= _sceneController.HandlePlayerDied;
            _playerRuntime.Dispose();
            _playerRuntime = null;
        }
    }
}
