using BillGameCore.Modules.Player.Application;
using BillGameCore.SharedPorts.Input;
using BillGameCore.Core.ValueObjects;
using BillGameCore.Core.Interaction;
using BillGameCore.Core.Combat;
using UnityEngine;
using System;

namespace BillGameCore.Modules.Player.Presentation
{
    public sealed class PlayerPresenter
    {
        private readonly PlayerApplication _application;
        private readonly PlayerView _view;     
        private readonly IInputCommandSource _inputCommandSource;
        private readonly BillEntityId _entityId;
        private IInteractable _currentInteractable;
        private Collider2D _currentOverlapCollider;
        private const float ContactDamage = 1f;

        public PlayerPresenter(PlayerApplication application, PlayerView view, IInputCommandSource inputCommandSource, BillEntityId entityId)
        {
            _application = application;
            _view = view;
            _entityId = entityId;
            _inputCommandSource = inputCommandSource;
            _view.TriggerEntered += HandleTriggerEntered;
            _view.TriggerExited += HandleTriggerExited;
        }

        public void Tick()
        {
            IMoveCommand latestMoveCommand = null;
            var interactRequested = false;
            var attackRequested = false;

            while (_inputCommandSource.TryDequeue(out var command))
            {
                if (command.ControlledEntityId != _entityId)
                {
                    continue;
                }

                if (command.Type == CommandType.Move)
                {
                    if (command is IMoveCommand moveCommand)
                    {
                        latestMoveCommand = moveCommand;
                    }

                    continue;
                }
                
                if (command.Type == CommandType.Attack
                    && command is IAttackCommand attackCommand
                    && attackCommand.IsPerformed)
                {
                    attackRequested = true;
                    continue;
                }

                if (command.Type == CommandType.Interact 
                    && command is IInteractCommand interactCommand 
                    && interactCommand.IsPerformed)
                {
                    interactRequested = true;
                    
                }
            }

            if (latestMoveCommand != null)
            {
                _application.ComputeMoveVelocity(
                    latestMoveCommand.DirX,
                    latestMoveCommand.DirY,
                    out var velocityX,
                    out var velocityY);

                _view.SetMoveVelocity(new Vector2(velocityX, velocityY));
            }
            if (attackRequested)
            {
                TryAttack();
            }
            if (interactRequested)
            {
                TryInteract();
            }
        }
        public Action OnDiedCallback { get; set; }
        public void Stop()
        {
            _view.SetMoveVelocity(Vector2.zero);
            
            _view.TriggerEntered -= HandleTriggerEntered;
            _view.TriggerExited -= HandleTriggerExited;
        }

        private void HandleTriggerEntered(Collider2D other)
        {
            if (other == null)
            {
                return;
            }

            _currentOverlapCollider = other;
            _currentInteractable = other.GetComponent<IInteractable>();
        }

        private void HandleTriggerExited(Collider2D other)
        {
            if (other == null)
            {
                return;
            }

            if (ReferenceEquals(other, _currentOverlapCollider))
            {
                _currentOverlapCollider = null;
                _currentInteractable = null;
            }
        }
        public DamageResult ReceiveDamage(DamageInfo damageInfo)
        {
            var result = _application.ReceiveDamage(damageInfo);

            if (result.JustDied)
            {
                OnDiedCallback?.Invoke();
            }

            return result;
        }
        private void TryAttack()
        {
            if (_currentOverlapCollider == null)
            {
                return;
            }

            var targetCollider = _currentOverlapCollider;
            var targetName = targetCollider.name;

            var damageReceiver = targetCollider.GetComponent<IDamageReceiver>();
            if (damageReceiver == null)
            {
                return;
            }

            var damageInfo = new DamageInfo(ContactDamage, _entityId, false);
            var damageResult = damageReceiver.ReceiveDamage(damageInfo);

            Debug.Log(
                $"Hit target '{targetName}' | Source={_entityId} | Damage={damageInfo.Amount} | Applied={damageResult.AppliedDamage} | RemainingHP={damageResult.RemainingHealth} | JustDied={damageResult.JustDied}",
                _view);
        }
        private void TryInteract()
        {
            if (_currentInteractable == null)
            {
                return;
            }

            if (!_currentInteractable.CanInteract())
            {
                return;
            }

            _currentInteractable.Interact();
        }
    }
}
