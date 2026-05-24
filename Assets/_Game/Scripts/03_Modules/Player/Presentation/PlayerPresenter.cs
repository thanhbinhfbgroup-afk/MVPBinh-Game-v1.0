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

                if (command.Type == CommandType.Interact // Thùng thư báo: "Có thư tương tác nè!"
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
            //HỦY ĐĂNG KÝ EVENT ĐỂ DỌN DẸP BỘ NHỚ RAM
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
            // Nếu không có vật thể, hoặc trong não Presenter vốn dĩ đang trống rỗng -> Thoát sớm cho nhẹ máy.
            if (other == null || _currentInteractable == null) return;

            // Lấy chiếc mặt nạ IInteractable của vật thể vừa đi ra ngoài
            var interactable = other.GetComponent<IInteractable>();

            // SO SÁNH VÙNG NHỚ: Cái vừa đi ra có đúng là cái đang được nhớ trong não không?
            if (ReferenceEquals(interactable, _currentInteractable))
            {
                _currentInteractable = null; // Đúng rồi thì xóa bộ nhớ về null!
            }
        }

        private void TryInteract()
        {   // Việc 1: Check null target
            if (_currentInteractable == null)
            {
                return;
            }
            // Việc 2: Check điều kiện có được tương tác không
            if (!_currentInteractable.CanInteract())
            {
                return;
            }
            // Việc 3: Gọi tương tác thực tế
            _currentInteractable.Interact();
        }
    }
}
