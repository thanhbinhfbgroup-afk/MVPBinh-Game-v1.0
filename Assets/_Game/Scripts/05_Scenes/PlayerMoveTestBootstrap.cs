using BillGameCore.Modules.Input.Commands;
using BillGameCore.Modules.Input.Infrastructure;
using BillGameCore.Modules.Player.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;

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
            _playerRuntime = spawner.Spawn(Vector2.zero);
        }

        private void Update()
        {
            var moveCommand = _inputReader.ReadMoveCommand();
         

            _playerRuntime.Tick(new Vector2(moveCommand.DirX, moveCommand.DirY));
        }
        
        private void OnDestroy()
        {
            _playerRuntime?.Dispose();
        }
    }

}