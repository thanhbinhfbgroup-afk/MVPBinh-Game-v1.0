using System;
using BillGameCore.Modules.Input.Commands;
using BillGameCore.SharedPorts.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BillGameCore.Modules.Input.Infrastructure
{
    public sealed class InputReader : MonoBehaviour
    {
        private const string PlayerActionMapName = "Player";
        private const string MoveActionName = "Move";

        [SerializeField] private InputActionAsset _actions;

        private InputActionMap _playerActionMap;
        private InputAction _moveAction;

        private void Awake()
        {
            ValidateConfiguration();
            CacheActions();
        }

        private void OnEnable()
        {
            _playerActionMap.Enable();
        }

        private void OnDisable()
        {
            _playerActionMap.Disable();
        }

        public void ValidateConfiguration()
        {
            if (_actions == null)
            {
                throw new InvalidOperationException("InputReader requires an InputActionAsset.");
            }

            var playerActionMap = _actions.FindActionMap(PlayerActionMapName, throwIfNotFound: false);
            if (playerActionMap == null)
            {
                throw new InvalidOperationException("InputReader could not find action map 'Player'.");
            }

            var moveAction = playerActionMap.FindAction(MoveActionName, throwIfNotFound: false);
            if (moveAction == null)
            {
                throw new InvalidOperationException("InputReader could not find action 'Player/Move'.");
            }
        }

        public IMoveCommand ReadMoveCommand()
        {
            var moveInput = _moveAction.ReadValue<Vector2>();
            return new MoveCommand(moveInput.x, moveInput.y);
        }

        private void CacheActions()
        {
            _playerActionMap = _actions.FindActionMap(PlayerActionMapName, throwIfNotFound: true);
            _moveAction = _playerActionMap.FindAction(MoveActionName, throwIfNotFound: true);
        }
    }
}