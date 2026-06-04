using BillGameCore.Modules.Player.Application;
using BillGameCore.SharedPorts.Input;
using BillGameCore.Core.ValueObjects;
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
        private Vector2 _lastNonZeroMoveDirection;
        private const float ContactDamage = 1f;

        public PlayerPresenter(PlayerApplication application, PlayerView view, IInputCommandSource inputCommandSource, BillEntityId entityId)
        {
            _application = application;
            _view = view;
            _entityId = entityId;
            _inputCommandSource = inputCommandSource;
        }

        public void Tick()
        {
            if (_application.IsDead)
            {
                _view.SetMoveVelocity(Vector2.zero);
                return;
            }
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
                if (latestMoveCommand.IsMoving)
                {
                    _lastNonZeroMoveDirection = new Vector2(
                        latestMoveCommand.DirX,
                        latestMoveCommand.DirY).normalized;
                }
                if (_lastNonZeroMoveDirection != Vector2.zero)
                {
                    _view.SetSensorFacingDirection(_lastNonZeroMoveDirection);
                }
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
            
        }
       
        public DamageResult ReceiveDamage(DamageInfo damageInfo)
        {
            var result = _application.ReceiveDamage(damageInfo);
            if (result.AppliedDamage > 0f)
            {
                Debug.Log(
                    $"Player took damage | Source={damageInfo.SourceId} | Damage={damageInfo.Amount} | Applied={result.AppliedDamage} | RemainingHP={result.RemainingHealth} | JustDied={result.JustDied}",
                    _view);
            }

            if (result.JustDied)
            {
                OnDiedCallback?.Invoke();
            }

            return result;
        }
        private void TryAttack()
        {
            if (!_view.TryGetNearestAttackTarget(out var targetCollider, out var damageReceiver))
            {
                return;
            }

            var targetName = targetCollider.name;

            var damageInfo = new DamageInfo(ContactDamage, _entityId, false);
            var damageResult = damageReceiver.ReceiveDamage(damageInfo);

            Debug.Log(
                $"Hit target '{targetName}' | Source={_entityId} | Damage={damageInfo.Amount} | Applied={damageResult.AppliedDamage} | RemainingHP={damageResult.RemainingHealth} | JustDied={damageResult.JustDied}",
                _view);

            if (damageResult.JustDied)
            {
                Debug.Log("Enemy died.", _view);
            }
        }
        private void TryInteract()
        {
            if (!_view.TryGetNearestInteractTarget(out var targetCollider, out var interactable))
            {
                return;
            }

            if (!interactable.CanInteract())
            {
                return;
            }

            interactable.Interact();
        }
    }
}
