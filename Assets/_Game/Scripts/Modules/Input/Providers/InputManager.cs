// [MODULE: Input]
// [TYPE: Manager]
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using MessagePipe;
using BillGameCore.Interfaces.Signals;

namespace BillGameCore.Modules.Input.Providers
{
    public class InputManager : MonoBehaviour
    {
        [Inject] private readonly IPublisher<OnMoveInputSignal> _publisher;
        private PlayerInput _playerInput;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
        }

        private void OnEnable()
        {
            // TỰ ĐỘNG NỐI DÂY BẰNG CODE:
            // Tìm action tên là "Move" và đăng ký hàm OnMove
            var moveAction = _playerInput.actions["Move"];
            moveAction.performed += OnMove;
            moveAction.canceled += OnMove;
        }

        private void OnDisable()
        {
            var moveAction = _playerInput.actions["Move"];
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMove;
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            if (_publisher == null) return;
            Vector2 input = context.ReadValue<Vector2>();
            _publisher.Publish(new OnMoveInputSignal(input));
        }
    }
}