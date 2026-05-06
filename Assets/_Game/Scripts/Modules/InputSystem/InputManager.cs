using BillGameCore.Core;
using BillGameCore.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;
using System;

namespace BillGameCore.InputSystem
{
    public class InputManager : IInputService, IInitializable, IDisposable, ITickable
    {
        private readonly InputData _config;
        private InputAction _moveAction;
        private InputAction _attackAction;
        private InputAction _interactAction;

        public Vector2 MoveDirection { get; private set; }
        public bool IsInputEnabled { get; private set; } = true;

        [Inject]
        public InputManager(InputData config) => _config = config;

        public void Initialize()
        {
            var map = _config.inputActions.actionMaps[0];
            _moveAction = map.FindAction(_config.moveActionName);
            _attackAction = map.FindAction(_config.attackActionName);
            _interactAction = map.FindAction(_config.interactActionName);

            _attackAction.performed += OnAttackPerformed;
            _interactAction.performed += OnInteractPerformed;

            _config.inputActions.Enable();
        }

        private void OnAttackPerformed(InputAction.CallbackContext ctx)
        { if (IsInputEnabled) EventBus.Publish(new InputAttackSignal()); }

        private void OnInteractPerformed(InputAction.CallbackContext ctx)
        { if (IsInputEnabled) EventBus.Publish(new InputInteractSignal()); }

        public void Tick()
        {
            if (!IsInputEnabled) { MoveDirection = Vector2.zero; return; }
            MoveDirection = _moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        }

        public void ToggleInput(bool isEnabled) => IsInputEnabled = isEnabled;

        public void Dispose()
        {
            _attackAction.performed -= OnAttackPerformed;
            _interactAction.performed -= OnInteractPerformed;
            _config.inputActions.Disable();
        }
    }
}