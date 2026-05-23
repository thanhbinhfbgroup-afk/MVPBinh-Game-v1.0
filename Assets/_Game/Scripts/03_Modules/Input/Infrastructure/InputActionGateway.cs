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

        public string CurrentContext { get; private set; }

        public InputActionGateway(InputActionAsset actions)
        {
            if (actions == null)
            {
                throw new InvalidOperationException("InputActionGateway requires an InputActionAsset.");
            }

            _runtimeActions = UnityEngine.Object.Instantiate(actions);
            _playerActionMap = _runtimeActions.FindActionMap(InputContextNames.Player, throwIfNotFound: true);
            _moveAction = _playerActionMap.FindAction("Move", throwIfNotFound: true);
        }

        public void SwitchContext(string contextName)
        {
            if (string.IsNullOrWhiteSpace(contextName))
            {
                throw new InvalidOperationException("Input context name cannot be null or empty.");
            }

            if (contextName == CurrentContext)
            {
                return;
            }

            _playerActionMap.Disable();

            switch (contextName)
            {
                case InputContextNames.Player:
                    _playerActionMap.Enable();
                    CurrentContext = InputContextNames.Player;
                    return;

                case InputContextNames.UI:
                case InputContextNames.Vehicle:
                    throw new InvalidOperationException($"Input context '{contextName}' is not supported yet.");

                default:
                    throw new InvalidOperationException($"Unknown input context '{contextName}'.");
            }
        }

        public void EnablePlayerMap()
        {
            SwitchContext(InputContextNames.Player);
        }

        public void DisablePlayerMap()
        {
            _playerActionMap.Disable();
            CurrentContext = string.Empty;
        }

        public Vector2 ReadMoveInput()
        {
            if (CurrentContext != InputContextNames.Player)
            {
                throw new InvalidOperationException(
                    $"Cannot read move input when current context is '{CurrentContext}'.");
            }

            return _moveAction.ReadValue<Vector2>();
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
