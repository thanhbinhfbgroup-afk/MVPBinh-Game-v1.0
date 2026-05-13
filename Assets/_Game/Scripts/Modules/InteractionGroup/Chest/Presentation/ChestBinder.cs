using BillGameCore.Core.Interaction;
using BillGameCore.Modules.InteractionGroup.Chest.Application;
using BillGameCore.Modules.InteractionGroup.Chest.Domain;
using BillGameCore.Modules.InteractionGroup.Chest.Infrastructure.Config;
using UnityEngine;

namespace BillGameCore.Modules.InteractionGroup.Chest.Presentation
{
    // Gắn trên prefab Chest trong scene. Implement IInteractable (Core).
    // PlayerPresenter gọi GetComponent<IInteractable>() — không bao giờ biết type này (R07).
    // Binder tự tạo Application/State/Presenter trong Awake — KHÔNG qua DI (R04).
    //
    // QUAN TRỌNG R17: Nếu thêm [VContainer.Inject] field (ví dụ IInventoryWriteService),
    //   BẮT BUỘC instantiate prefab qua container.Instantiate(), không dùng Object.Instantiate().
    public sealed class ChestBinder : MonoBehaviour, IInteractable
    {
        [SerializeField] private ChestView   _view;
        [SerializeField] private ChestConfig _config;

        // Thêm [VContainer.Inject] service field ở đây nếu cần.
        // Nhớ đổi sang container.Instantiate() trong spawner (R17).

        private ChestApplication _app;
        private ChestPresenter   _presenter;

        private void Awake()
        {
            if (_view == null) _view = GetComponent<ChestView>();
            var def   = _config != null ? _config.ToDefinition() : new ChestDefinition();
            var state = new ChestState();
            _app       = new ChestApplication(def, state);
            _presenter = new ChestPresenter(_app, _view);

            if (_app.IsActivated) _view.PlayActivated();
        }

        public bool CanInteract() => _app != null && !_app.IsActivated;

        public void Interact()
        {
            _presenter?.TryActivate();
            // Thêm service call ở đây (ví dụ _inventory.AddItem(...)) sau khi thêm [Inject].
        }

        private void OnDestroy() => _presenter?.Dispose();
    }
}