using BillGameCore.Modules.Player.Application;
using BillGameCore.SharedPorts.Input;
using BillGameCore.Core.ValueObjects;
using BillGameCore.Core.Interaction;
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
           
            _currentInteractable = other.GetComponent<IInteractable>();
        }

        private void HandleTriggerExited(Collider2D other)
        {
            
            if (other == null || _currentInteractable == null) return;

            
            var interactable = other.GetComponent<IInteractable>();

            
            if (ReferenceEquals(interactable, _currentInteractable))
            {
                _currentInteractable = null; 
            }
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
