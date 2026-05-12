using System;
using BillGameCore.Core.Interaction;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;
using BillGameCore.Modules.Player.Application;
using BillGameCore.Modules.Input.Commands;
using BillGameCore.SharedPorts.Input;
using UnityEngine;

namespace BillGameCore.Modules.Player.Presentation
{
    // Bridges IInputCommandSource → PlayerApplication → PlayerView each frame.
    // Constructed by PlayerSpawner — NOT by DI container (R04).
    public sealed class PlayerPresenter : IDisposable
    {
        private readonly PlayerApplication  _app;
        private readonly PlayerView         _view;
        private readonly IInputCommandSource _input;

        private bool  _deathPlayed;
        private float _lastDirX, _lastDirY;

        // Wired by SceneController so Presenter has no knowledge of LootSpawner etc.
        public Action<EntityId> OnDiedCallback;

        public PlayerPresenter(PlayerApplication app, PlayerView view, IInputCommandSource input)
        {
            _app   = app;
            _view  = view;
            _input = input;
            _app.OnDied += HandleDied;
        }

        // Called from PlayerView.Update() — not from scene code directly.
        public void OnUpdate(float deltaTime)
        {
            if (_app.IsDead)
            {
                if (!_deathPlayed) { _deathPlayed = true; _view.PlayDeath(); }
                return;
            }

            while (_input.TryDequeue(out ICommand cmd))
            {
                switch (cmd)
                {
                    case MoveCommand mv:
                        _lastDirX = mv.DirX;
                        _lastDirY = mv.DirY;
                        break;
                    case AttackCommand _:
                        // TODO (Slice 03): call ICombatService.RequestAttack()
                        break;
                    case InteractCommand _:
                        // Handled in OnTriggerEnter2D
                        break;
                }
            }

            _app.Tick(_lastDirX, _lastDirY, deltaTime);
            _view.UpdateMoveAnimation(_lastDirX, _lastDirY);
        }

        // Called from PlayerView.FixedUpdate() — physics write here, not in Update.
        public void OnFixedUpdate() => _view.SetVelocity(_app.VelocityX, _app.VelocityY);

        // Called from PlayerView.OnTriggerEnter2D() — R03 forward pattern.
        public void OnTriggerEnter2D(Collider2D col)
        {
            var interactable = col.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract())
                interactable.Interact();
        }

        public void Dispose() => _app.OnDied -= HandleDied;

        private void HandleDied(EntityId id) => OnDiedCallback?.Invoke(id);
    }
}