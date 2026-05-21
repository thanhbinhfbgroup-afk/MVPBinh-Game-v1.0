using BillGameCore.Modules.Input.Infrastructure;
using BillGameCore.Modules.Player.Presentation;
using BillGameCore.SharedPorts.Input;
using UnityEngine;

namespace BillGameCore.Scenes
{
    public sealed class PlayerMoveTestBootstrap : MonoBehaviour
    {
        [SerializeField] private PlayerView _playerPrefab;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private InputReader _inputReader;

        private PlayerRuntime _playerRuntime;

        private void Awake()
        {
            var spawner = new PlayerSpawner(_playerPrefab, _moveSpeed);
            _playerRuntime = spawner.Spawn(Vector2.zero, _inputReader);
        }

        private void Update()
        {
            _playerRuntime.Tick();
        }
        
        private void OnDestroy()
        {
            _playerRuntime?.Dispose();
        }
    }

}