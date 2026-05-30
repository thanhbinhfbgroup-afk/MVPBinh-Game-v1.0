using BillGameCore.Modules.Economy.Application;
using BillGameCore.Modules.Input.Application;
using BillGameCore.Modules.Input.Commands;
using BillGameCore.Modules.Input.Context;
using BillGameCore.Modules.Input.Infrastructure;
using BillGameCore.Modules.InteractionGroup.Chest.Presentation;
using BillGameCore.Modules.Player.Presentation;
using BillGameCore.SharedPorts.Economy;
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
        [SerializeField] private ChestBinder _testChest;


        private PlayerRuntime _playerRuntime;
        private RewardGrantService _rewardGrantService;

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
        [ContextMenu("Debug/Log Wallet")]
        private void DebugLogWallet()
        {
            if (_rewardGrantService == null)
            {
                Debug.LogWarning("RewardGrantService has not been created yet.", this);
                return;
            }

            Debug.Log(
                $"Wallet => Gold={_rewardGrantService.Gold}, Experience={_rewardGrantService.Experience}.",
                this);
        }
        private void Awake()
        {
            var commandBuffer = new CommandBuffer();
            _inputReader.SetCommandBuffer(commandBuffer);

            var inputCommandSource = new InputCommandDispatcher(commandBuffer);

            var spawner = new PlayerSpawner(_playerPrefab, _moveSpeed, inputCommandSource);
            _playerRuntime = spawner.Spawn(Vector2.zero);
            _inputReader.SetControlledEntity(_playerRuntime.Id);
            _playerRuntime.SetOnDiedCallback(_sceneController.HandlePlayerDied);
            _rewardGrantService = new RewardGrantService();
            _testChest.SetRewardGrantService(_rewardGrantService);

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
