using BillGameCore.Modules.Economy.Application;
using BillGameCore.Modules.Input.Infrastructure;
using BillGameCore.Modules.InteractionGroup.ItemPickup.Presentation;
using BillGameCore.Modules.InteractionGroup.Chest.Presentation;
using BillGameCore.Modules.Player.Presentation;
using BillGameCore.Modules.Enemy.Presentation;
using BillGameCore.Scenes.UI;
using BillGameCore.SharedPorts.Inventory;
using System.Collections.Generic;
using TMPro;
using VContainer;
using UnityEngine;

namespace BillGameCore.Scenes
{
    public sealed class SceneBootstrapper : MonoBehaviour
    {
        private const string DefaultInventoryItemId = "wood";
        private const string DefaultInventoryItemLabel = "Wood";

        [SerializeField] private ChestBinder[] _chests;
        [SerializeField] private EnemyView[] _enemyViews;


        private PlayerSpawner _playerSpawner;
        private InputReader _inputReader;
        private SceneController _sceneController;
        private PlayerRuntime _playerRuntime;
        private RewardGrantService _rewardGrantService;
        private WalletHudPresenter _walletHudPresenter;
        private WalletHudView _walletHudView;
        private EnemySpawner _enemySpawner;
        private IInventoryReadService _inventoryReadService;
        private IInventoryWriteService _inventoryWriteService;
        private InventoryHudPresenter _inventoryHudPresenter;
        private readonly List<EnemyRuntime> _enemyRuntimes = new();
        private static Sprite s_defaultPickupSprite;

        [Inject]
        public void Construct(PlayerSpawner playerSpawner, InputReader inputReader, SceneController sceneController, 
            RewardGrantService rewardGrantService, WalletHudPresenter walletHudPresenter, WalletHudView walletHudView, EnemySpawner enemySpawner,
            IInventoryReadService inventoryReadService, IInventoryWriteService inventoryWriteService)
        {
            _playerSpawner = playerSpawner;
            _inputReader = inputReader;
            _sceneController = sceneController;
            _rewardGrantService = rewardGrantService;
            _walletHudPresenter = walletHudPresenter;
            _walletHudView = walletHudView;
            _enemySpawner = enemySpawner;
            _inventoryReadService = inventoryReadService;
            _inventoryWriteService = inventoryWriteService;
        }

        private void Awake()
        {
            _playerRuntime = _playerSpawner.Spawn(Vector2.zero);
            _inputReader.SetControlledEntity(_playerRuntime.Id);
            _playerRuntime.SetOnDiedCallback(_sceneController.HandlePlayerDied);
            _walletHudPresenter.Refresh();
            _inventoryHudPresenter = CreateInventoryHudPresenter();
            _inventoryHudPresenter.Refresh();
            _sceneController.SetRewardGrantService(_rewardGrantService);

            InitializeEnemyRuntimes();
            WireChestCallbacks();
            CreateWoodPickup();
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

        private InventoryHudPresenter CreateInventoryHudPresenter()
        {
            var hudObject = new GameObject(
                "InventoryHudText",
                typeof(RectTransform),
                typeof(TextMeshProUGUI),
                typeof(InventoryHudView));

            var parent = _walletHudView.transform.parent != null
                ? _walletHudView.transform.parent
                : _walletHudView.transform;

            hudObject.transform.SetParent(parent, false);

            var rectTransform = (RectTransform)hudObject.transform;
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(24f, -120f);
            rectTransform.sizeDelta = new Vector2(320f, 40f);

            var text = hudObject.GetComponent<TextMeshProUGUI>();
            text.fontSize = 28f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.raycastTarget = false;

            var view = hudObject.GetComponent<InventoryHudView>();
            return new InventoryHudPresenter(
                _inventoryReadService,
                view,
                DefaultInventoryItemId,
                DefaultInventoryItemLabel);
        }

        private void CreateWoodPickup()
        {
            var pickupObject = new GameObject("WoodPickup");
            pickupObject.layer = ResolveInteractableLayer();
            pickupObject.transform.position = new Vector3(1.5f, 0f, 0f);
            pickupObject.transform.localScale = new Vector3(0.35f, 0.35f, 1f);

            var spriteRenderer = pickupObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetDefaultPickupSprite();
            spriteRenderer.color = new Color(0.62f, 0.39f, 0.18f, 1f);
            spriteRenderer.sortingOrder = 5;

            var triggerCollider = pickupObject.AddComponent<CircleCollider2D>();
            triggerCollider.isTrigger = true;
            triggerCollider.radius = 0.5f;

            pickupObject.AddComponent<ItemPickupView>();

            var binder = pickupObject.AddComponent<ItemPickupBinder>();
            binder.Construct(_inventoryWriteService);
            binder.Configure(DefaultInventoryItemId, 1);
            binder.SetPickedUpCallback(_inventoryHudPresenter.Refresh);
        }

        private static int ResolveInteractableLayer()
        {
            var interactableLayer = LayerMask.NameToLayer("Interactable");
            return interactableLayer >= 0 ? interactableLayer : 0;
        }

        private static Sprite GetDefaultPickupSprite()
        {
            if (s_defaultPickupSprite != null)
            {
                return s_defaultPickupSprite;
            }

            var texture = Texture2D.whiteTexture;
            s_defaultPickupSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                texture.width);

            return s_defaultPickupSprite;
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
