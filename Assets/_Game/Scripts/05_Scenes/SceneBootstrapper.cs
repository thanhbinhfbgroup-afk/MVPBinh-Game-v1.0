using BillGameCore.Modules.Economy.Application;
using BillGameCore.Modules.Input.Infrastructure;
using BillGameCore.Modules.InteractionGroup.Chest.Presentation;
using BillGameCore.Modules.Player.Presentation;
using BillGameCore.Modules.Enemy.Presentation;
using BillGameCore.Scenes.UI;
using System.Collections.Generic;
using VContainer;
using UnityEngine;

namespace BillGameCore.Scenes
{
    public sealed class SceneBootstrapper : MonoBehaviour
    {
        [SerializeField] private ChestBinder[] _chests;
        [SerializeField] private EnemyView[] _enemyViews;


        private PlayerSpawner _playerSpawner;
        private InputReader _inputReader;
        private SceneController _sceneController;
        private PlayerRuntime _playerRuntime;
        private RewardGrantService _rewardGrantService;
        private WalletHudPresenter _walletHudPresenter;
        private EnemySpawner _enemySpawner;
        private readonly List<EnemyRuntime> _enemyRuntimes = new();

        [Inject]
        public void Construct(PlayerSpawner playerSpawner, InputReader inputReader, SceneController sceneController, 
            RewardGrantService rewardGrantService, WalletHudPresenter walletHudPresenter, EnemySpawner enemySpawner)
        {
            _playerSpawner = playerSpawner;
            _inputReader = inputReader;
            _sceneController = sceneController;
            _rewardGrantService = rewardGrantService;
            _walletHudPresenter = walletHudPresenter;
            _enemySpawner = enemySpawner;
        }

        private void Awake()
        {
            _playerRuntime = _playerSpawner.Spawn(Vector2.zero);
            _inputReader.SetControlledEntity(_playerRuntime.Id);
            _playerRuntime.SetOnDiedCallback(_sceneController.HandlePlayerDied);
            _walletHudPresenter.Refresh();
            _sceneController.SetRewardGrantService(_rewardGrantService);

            InitializeEnemyRuntimes();
            WireChestCallbacks();
        }

        private void Update()
        {
            _playerRuntime.Tick();

            var playerWorldPosition = _playerRuntime.View.WorldPosition;

            for (var i = 0; i < _enemyRuntimes.Count; i++)
            {
                _enemyRuntimes[i].Tick(playerWorldPosition);
            }
        }
        private void InitializeEnemyRuntimes()
        {
            if (_enemyViews == null)
            {
                return;
            }

            foreach (var enemyView in _enemyViews)
            {
                if (enemyView == null)
                {
                    continue;
                }

                var enemyRuntime = _enemySpawner.Spawn(enemyView);
                enemyRuntime.Presenter.OnDiedCallback = reward =>
                {
                    _sceneController.HandleEnemyDied(reward);
                    _walletHudPresenter.Refresh();
                };

                _enemyRuntimes.Add(enemyRuntime);
            }
        }

        private void WireChestCallbacks()
        {
            foreach (var chest in _chests)
            {
                if (chest == null)
                {
                    continue;
                }

                chest.SetOpenedCallback(_walletHudPresenter.Refresh);
            }
        }
        private void OnDestroy()
        {
            _playerRuntime?.Dispose();
            for (var i = 0; i < _enemyRuntimes.Count; i++)
            {
                _enemyRuntimes[i]?.Dispose();
            }
        }
        
    }
}
