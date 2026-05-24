using BillGameCore.Modules.Input.Context;
using BillGameCore.SharedPorts.Input;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BillGameCore.Modules.Input.Infrastructure
{
    public sealed class InputActionGateway : IDisposable
    {
        private readonly InputActionAsset _runtimeActions;
        private readonly InputActionMap _playerActionMap;
        private readonly InputAction _moveAction;
        private readonly InputAction _interactAction;
        public InputContext CurrentContext { get; private set; }

        public InputActionGateway(InputActionAsset actions)
        {
            if (actions == null)
            {
                throw new InvalidOperationException("InputActionGateway requires an InputActionAsset.");
            }

            _runtimeActions = UnityEngine.Object.Instantiate(actions);
            _playerActionMap = _runtimeActions.FindActionMap(InputContextNames.Player, throwIfNotFound: true);
            _moveAction = _playerActionMap.FindAction("Move", throwIfNotFound: true);
            _interactAction = _playerActionMap.FindAction("Interact", throwIfNotFound: true);
        }

        // Hàm chuyển đổi ngữ cảnh (Nhận vào struct InputContext an toàn)
        public void SwitchContext(InputContext context)
        {
            // CHẶN: Nếu ngữ cảnh truyền vào là Rỗng/Vô giá trị -> Nổ lỗi ngay
            if (context == InputContext.None)
            {
                throw new InvalidOperationException("Input context cannot be None.");
            }

            // CHẶN: Nếu trùng khớp với ngữ cảnh hiện tại -> Thoát luôn cho nhẹ máy
            if (context == CurrentContext)
            {
                return;
            }

            // TẮT: Ngắt hoàn toàn sơ đồ phím hành động của Player hiện tại
            _playerActionMap.Disable();

            // BẮT ĐẦU: Rẽ nhánh để kích hoạt map phím mới
            switch (context)
            {
                // Case Player: Bật phím di chuyển + Đổi biển trạng thái hệ thống
                case InputContext.Player:
                    _playerActionMap.Enable();
                    CurrentContext = InputContext.Player;
                    return;

                // Case tương lai (UI/Xe cộ): Chặn đứng vì hiện tại chưa viết code xử lý
                case InputContext.UI:
                case InputContext.Vehicle:
                    throw new InvalidOperationException($"Input context '{context}' is not supported yet.");

                // Case bậy bạ: Báo động nếu lọt vào một ngữ cảnh lạ hoắc ngoài thiết kế
                default:
                    throw new InvalidOperationException($"Unknown input context '{context}'.");
            }
        }

        public void EnablePlayerMap()
        {
            SwitchContext(InputContext.Player);
        }

        public void DisablePlayerMap()
        {
            _playerActionMap.Disable();
            CurrentContext = InputContext.None;
        }

        public Vector2 ReadMoveInput()
        {
            if (CurrentContext != InputContext.Player)
            {
                throw new InvalidOperationException(
                    $"Cannot read move input when current context is '{CurrentContext}'.");
            }

            return _moveAction.ReadValue<Vector2>();
        }
        public bool WasInteractPerformedThisFrame()
        {
            if (CurrentContext != InputContext.Player)
            {
                throw new InvalidOperationException(
                    $"Cannot read interact input when current context is '{CurrentContext}'.");
            }

            return _interactAction.WasPerformedThisFrame();
        }
        public void Dispose()
        {
            if (_runtimeActions != null)
            {
                UnityEngine.Object.Destroy(_runtimeActions);
            }
        }
    }
}
