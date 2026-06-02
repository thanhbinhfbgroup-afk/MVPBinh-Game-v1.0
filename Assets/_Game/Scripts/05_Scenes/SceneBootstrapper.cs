using BillGameCore.Modules.Economy.Application;
using BillGameCore.Modules.Input.Application;
using BillGameCore.Modules.Input.Commands;
using BillGameCore.Modules.Input.Context;
using BillGameCore.Modules.Input.Infrastructure;
using BillGameCore.Modules.InteractionGroup.Chest.Presentation;
using BillGameCore.Modules.Player.Presentation;
using BillGameCore.SharedPorts.Economy;
using BillGameCore.SharedPorts.Input;
using BillGameCore.Modules.Enemy.Infrastructure.Config;
using BillGameCore.Modules.Enemy.Presentation;
using BillGameCore.Scenes.UI;
using BillGameCore.Core.Combat;
using BillGameCore.Core.ValueObjects;
using UnityEngine;

namespace BillGameCore.Scenes
{
    public sealed class SceneBootstrapper : MonoBehaviour
    {
        [SerializeField] private PlayerView _playerPrefab;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private InputReader _inputReader;
        [SerializeField] private SceneController _sceneController;
        [SerializeField] private ChestBinder[] _chests;
        [SerializeField] private WalletHudView _walletHudView;
        [SerializeField] private EnemyConfig _enemyConfig;
        [SerializeField] private EnemyView _enemyView;



        private PlayerRuntime _playerRuntime;
        private RewardGrantService _rewardGrantService;
        private WalletHudPresenter _walletHudPresenter;
        private EnemySpawner _enemySpawner;
        private EnemyRuntime _enemyRuntime;

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
                $"Wallet => Gold={_rewardGrantService.Gold}, XP={_rewardGrantService.Experience}.",
                this);
        }
        [ContextMenu("Debug/Damage Player 1 HP")]
        private void DebugDamagePlayer()
        {
            if (_playerRuntime == null)
            {
                Debug.LogWarning("PlayerRuntime has not been created yet.", this);
                return;
            }

            var damageInfo = new DamageInfo(1f, BillEntityId.Invalid, false);
            var damageResult = _playerRuntime.ReceiveDamage(damageInfo);

            Debug.Log(
                $"Debug damage player | Applied={damageResult.AppliedDamage} | RemainingHP={damageResult.RemainingHealth} | JustDied={damageResult.JustDied}",
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
            _walletHudPresenter = new WalletHudPresenter(_rewardGrantService, _walletHudView);
            _walletHudPresenter.Refresh();
            _sceneController.SetEnemyDeathRewardFlow(_rewardGrantService, _walletHudPresenter);
            _enemySpawner = new EnemySpawner(_enemyConfig, _enemyView);
            _enemyRuntime = _enemySpawner.Spawn();
            _enemyRuntime.Presenter.OnDiedCallback = _sceneController.HandleEnemyDied;

            foreach (var chest in _chests)
            {
                if (chest == null)
                {
                    continue;
                }

                chest.SetRewardGrantService(_rewardGrantService);
                chest.SetOpenedCallback(_walletHudPresenter.Refresh);
            }

        }

        private void Update()
        {
            _playerRuntime.Tick();
        }

        private void OnDestroy()
        {
            _playerRuntime?.Dispose();
            _enemyRuntime?.Dispose();
        }
        
    }
}
