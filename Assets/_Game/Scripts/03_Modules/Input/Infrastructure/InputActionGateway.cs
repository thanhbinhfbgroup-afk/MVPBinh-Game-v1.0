using System;
using BillGameCore.Modules.Input.Context;
using BillGameCore.SharedPorts.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BillGameCore.Modules.Input.Infrastructure
{
    public sealed class InputActionGateway : IDisposable
    {
        private const string MoveActionName = "Move";
        private const string AttackActionName = "Attack";
        private const string InteractActionName = "Interact";

        private readonly InputActionAsset _runtimeAsset;
        private readonly InputActionMap _playerMap;
        private readonly InputAction _moveAction;
        private readonly InputAction _attackAction;
        private readonly InputAction _interactAction;

        public InputActionGateway(InputActionAsset sourceAsset)
        {
            if (sourceAsset == null)
            {
                throw new ArgumentNullException(nameof(sourceAsset));
            }

            _runtimeAsset = UnityEngine.Object.Instantiate(sourceAsset);

            _playerMap = FindRequiredMap(InputContextNames.Player);
            _moveAction = FindRequiredAction(_playerMap, MoveActionName);
            _attackAction = FindRequiredAction(_playerMap, AttackActionName);
            _interactAction = FindRequiredAction(_playerMap, InteractActionName);

            CurrentContext = InputContext.None;
        }

        public InputContext CurrentContext { get; private set; }

        public void EnableContext(InputContext context)
        {
            if (context == InputContext.None)
            {
                DisableAll();
                CurrentContext = InputContext.None;
                return;
            }

            var mapName = InputContextNames.ToActionMapName(context);

            DisableAll();

            if (mapName == InputContextNames.Player)
            {
                _playerMap.Enable();
                CurrentContext = context;
                return;
            }

            throw new ArgumentOutOfRangeException(nameof(context), context, "Input context is declared but not implemented yet.");
        }

        public Vector2 ReadMove()
        {
            EnsureContext(InputContext.Player);
            return _moveAction.ReadValue<Vector2>();
        }

        public void Dispose()
        {
            DisableAll();
            UnityEngine.Object.Destroy(_runtimeAsset);
        }

        private InputActionMap FindRequiredMap(string mapName)
        {
            var map = _runtimeAsset.FindActionMap(mapName, throwIfNotFound: false);

            if (map == null)
            {
                throw new InvalidOperationException($"Input action map '{mapName}' was not found.");
            }

            return map;
        }

        private static InputAction FindRequiredAction(InputActionMap map, string actionName)
        {
            var action = map.FindAction(actionName, throwIfNotFound: false);

            if (action == null)
            {
                throw new InvalidOperationException($"Input action '{map.name}/{actionName}' was not found.");
            }

            return action;
        }

        private void EnsureContext(InputContext expectedContext)
        {
            if (CurrentContext != expectedContext)
            {
                throw new InvalidOperationException($"Input context must be '{expectedContext}', but current context is '{CurrentContext}'.");
            }
        }

        private void DisableAll()
        {
            _runtimeAsset.Disable();
        }
    }
}