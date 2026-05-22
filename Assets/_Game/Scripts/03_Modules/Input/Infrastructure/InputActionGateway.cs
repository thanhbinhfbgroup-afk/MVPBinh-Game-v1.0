using System;
using BillGameCore.Modules.Input.Context;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BillGameCore.Modules.Input.Infrastructure
{
    public sealed class InputActionGateway : IDisposable
    {
        private readonly InputActionAsset _runtimeActions;
        private readonly InputActionMap _playerActionMap;
        private readonly InputAction _moveAction;

        public InputActionGateway(InputActionAsset actions)
        {
            if (actions == null)
            {
                throw new InvalidOperationException("InputActionGateway requires an InputActionAsset.");
            }
            // Nhân bản cấu hình phím thành bản sao trên RAM(Runtime).
            // Giúp cô lập dữ liệu: chỉnh sửa phím khi chơi không bị ghi đè/làm hỏng file Asset gốc trên ổ cứng.
            _runtimeActions = UnityEngine.Object.Instantiate(actions);

            _playerActionMap = _runtimeActions.FindActionMap(InputContextNames.Player, throwIfNotFound: true);
            _moveAction = _playerActionMap.FindAction("Move", throwIfNotFound: true);
        }

        public void EnablePlayerMap()
        {
            _playerActionMap.Enable();
        }

        public void DisablePlayerMap()
        {
            _playerActionMap.Disable();
        }

        public Vector2 ReadMoveInput()
        {
            return _moveAction.ReadValue<Vector2>();
        }

        public void Dispose()
        {
            if (_runtimeActions != null)
            {
                UnityEngine.Object.Destroy(_runtimeActions); // Dọn sạch bản sao trên RAM, trả lại bộ nhớ cho máy tính
            }
        }
    }
}