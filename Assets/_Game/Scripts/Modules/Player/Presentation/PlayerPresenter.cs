using System;
using BillGameCore.Core.Interaction;
using BillGameCore.Core.Rewards;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;
using BillGameCore.Modules.Player.Application;
using BillGameCore.SharedPorts.Combat;
using BillGameCore.SharedPorts.Input;
using UnityEngine;


namespace BillGameCore.Modules.Player.Presentation
{
    // Bridge IInputCommandSource → PlayerApplication → PlayerView mỗi frame.
    // Được tạo bởi PlayerSpawner — KHÔNG qua DI container (R04).
    //
    // FIX-03: Dùng CommandType enum + interface IMoveCommand/IAttackCommand (SharedPorts).
    //         KHÔNG tham chiếu BillGameCore.Modules.Input.Commands namespace (R06/R07).
    // FIX-05: Signature OnDiedCallback khớp với Application.OnDied (§14C).
    public sealed class PlayerPresenter : IDisposable
    {
        private readonly PlayerApplication      _app;
        private readonly PlayerView             _view;
        private readonly IInputCommandSource _input;
        private readonly ICombatService      _combat; // null cho đến khi Slice 03 được merge

        private bool  _deathPlayed;
        private float _lastDirX, _lastDirY;
        private IInteractable _currentInteractable;

        // FIX-05: Callback mang (EntityId, RewardBundle, Vector2) khớp với
        //         EnemyApplication.OnDied và SceneController.HandleEnemyDied.
        public Action<EntityId, RewardBundle, Vector2> OnDiedCallback;

        // Constructor khi ICombatService chưa có (Slice 01-02).
        public PlayerPresenter(PlayerApplication app, PlayerView view, IInputCommandSource input)
            : this(app, view, input, null) { }

        // Constructor từ Slice 03 trở đi khi ICombatService đã có.
        public PlayerPresenter(PlayerApplication app, PlayerView view,
                            IInputCommandSource input, ICombatService combat)
        {
            _app    = app;
            _view   = view;
            _input  = input;
            _combat = combat;
            _app.OnDied += HandleDied;
        }

        // Gọi từ PlayerView.Update() — không gọi trực tiếp từ scene code.
        public void OnUpdate(float deltaTime)
        {
            if (_app.IsDead)
            {
                if (!_deathPlayed) { _deathPlayed = true; _view.PlayDeath(); }
                return;
            }

            // FIX-03: Chỉ dùng CommandType enum và cast sang SharedPorts interface.
            //         KHÔNG dùng kiểu MoveCommand/AttackCommand cụ thể (R06/R07).
            while (_input.TryDequeue(out ICommand cmd))
            {
                switch (cmd.Type)
                {
                    case CommandType.Move:
                        // Cast sang IMoveCommand (SharedPorts) để đọc DirX/Y (FIX-03).
                        if (cmd is IMoveCommand mv)
                        {
                            _lastDirX = mv.DirX;
                            _lastDirY = mv.DirY;
                        }
                        break;

                    case CommandType.Attack:
                        // TODO Slice 03: gọi _combat?.RequestAttack(...)
                        // if (cmd is IAttackCommand atk) { ... }
                        break;

                    case CommandType.Interact:
                        TryInteract();
                        break;
                }
            }

            _app.Tick(_lastDirX, _lastDirY, deltaTime);
            _view.UpdateMoveAnimation(_lastDirX, _lastDirY);
        }

        // Gọi từ PlayerView.FixedUpdate() — write physics ở đây, không trong Update.
        public void OnFixedUpdate() => _view.SetVelocity(_app.VelocityX, _app.VelocityY);

        // Gọi từ PlayerView.OnTriggerEnter2D() — pattern forward R03.
        // PlayerPresenter gọi GetComponent<IInteractable>() — không biết kiểu Binder (R07).
        public void OnTriggerEnter2D(Collider2D col)
        {
            var interactable = col.GetComponent<IInteractable>();
            if (interactable != null)
                _currentInteractable = interactable;
        }

        public void OnTriggerExit2D(Collider2D col)
        {
            var interactable = col.GetComponent<IInteractable>();
            if (interactable != null && ReferenceEquals(interactable, _currentInteractable))
                _currentInteractable = null;
        }

        public void Dispose() => _app.OnDied -= HandleDied;

        // Lấy vị trí thực từ View vì Application không biết Unity world position.
        private void HandleDied(EntityId id, RewardBundle bundle)
        {
            var worldPos = _view != null ? _view.WorldPosition : Vector2.zero;
            OnDiedCallback?.Invoke(id, bundle, worldPos);
        }

        private void TryInteract()
        {
            if (_currentInteractable != null && _currentInteractable.CanInteract())
                _currentInteractable.Interact();
        }
    }
}
