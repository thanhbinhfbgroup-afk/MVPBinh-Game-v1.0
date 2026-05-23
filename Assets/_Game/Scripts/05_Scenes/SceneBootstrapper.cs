using BillGameCore.Modules.Input.Application;
using BillGameCore.Modules.Input.Commands;
using BillGameCore.Modules.Input.Context;
using BillGameCore.Modules.Input.Infrastructure;
using BillGameCore.Modules.Player.Presentation;
using BillGameCore.SharedPorts.Input;
using UnityEngine;

namespace BillGameCore.Scenes
{
    public sealed class SceneBootstrapper : MonoBehaviour
    {
        [SerializeField] private PlayerView _playerPrefab;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private InputReader _inputReader;
        [SerializeField] private SceneController _sceneController;

        private PlayerRuntime _playerRuntime;

        [ContextMenu("Debug/Switch Context To Player")]
        private void DebugSwitchContextToPlayer()
        {
            var command = new SwitchContextCommand(_playerRuntime.Id, InputContext.Player);
            _inputReader.ConsumeSwitchContext(command);
        }

        [ContextMenu("Debug/Switch Context To UI")]
        private void DebugSwitchContextToUI()
        {
            var command = new SwitchContextCommand(_playerRuntime.Id, InputContext.UI);
            _inputReader.ConsumeSwitchContext(command);
        }
        private void Awake()
        {
            var commandBuffer = new CommandBuffer();
            _inputReader.SetCommandBuffer(commandBuffer);

            var inputCommandSource = new InputCommandDispatcher(commandBuffer);

            var spawner = new PlayerSpawner(_playerPrefab, _moveSpeed);
            _playerRuntime = spawner.Spawn(Vector2.zero, inputCommandSource);
            _inputReader.SetControlledEntity(_playerRuntime.Id);
            _playerRuntime.SetOnDiedCallback(_sceneController.HandlePlayerDied);
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
