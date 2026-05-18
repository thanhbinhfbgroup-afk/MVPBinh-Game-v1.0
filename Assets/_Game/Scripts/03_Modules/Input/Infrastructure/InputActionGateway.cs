using System;
using BillGameCore.Modules.Input.Context;
using BillGameCore.SharedPorts.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BillGameCore.Modules.Input.Infrastructure
{
    // Owns a runtime clone of InputActionAsset and exposes typed reads to InputReader.
    // R21: không dùng PlayerInput component và không dùng generated C# wrapper.
    public sealed class InputActionGateway : IDisposable
    {
        private const string MoveActionName = "Move";
        private const string AttackActionName = "Attack";
        private const string InteractActionName = "Interact";

        private readonly InputActionAsset _runtimeAsset;

        private readonly InputActionMap _playerMap;
        private readonly InputActionMap _vehicleMap;
        private readonly InputActionMap _uiMap;

        private readonly InputAction _playerMove;
        private readonly InputAction _playerAttack;
        private readonly InputAction _playerInteract;

        public InputActionGateway(InputActionAsset sourceAsset)
        {
            if (sourceAsset == null)
            {
                throw new ArgumentNullException(nameof(sourceAsset));
            }

            _runtimeAsset = UnityEngine.Object.Instantiate(sourceAsset);

            _playerMap = RequireMap(InputContextNames.Player);
            _vehicleMap = FindMap(InputContextNames.Vehicle);
            _uiMap = FindMap(InputContextNames.UI);

            _playerMove = RequireAction(_playerMap, MoveActionName);
            _playerAttack = RequireAction(_playerMap, AttackActionName);
            _playerInteract = RequireAction(_playerMap, InteractActionName);

            SwitchContext(InputContext.Player);
        }

        public InputContext CurrentContext { get; private set; }

        public void SwitchContext(InputContext context)
        {
            DisableAllMaps();

            switch (context)
            {
                case InputContext.Player:
                    _playerMap.Enable();
                    break;

                case InputContext.Vehicle:
                    RequireContextMap(_vehicleMap, InputContextNames.Vehicle).Enable();
                    break;

                case InputContext.UI:
                    RequireContextMap(_uiMap, InputContextNames.UI).Enable();
                    break;

                case InputContext.None:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(context),
                        context,
                        "Unsupported input context.");
            }

            CurrentContext = context;
        }

        public Vector2 ReadPlayerMove()
        {
            return _playerMove.ReadValue<Vector2>();
        }

        public bool WasPlayerAttackPressedThisFrame()
        {
            return _playerAttack.WasPressedThisFrame();
        }

        public bool WasPlayerAttackReleasedThisFrame()
        {
            return _playerAttack.WasReleasedThisFrame();
        }

        public bool IsPlayerAttackHeld()
        {
            return _playerAttack.IsPressed();
        }

        public bool WasPlayerInteractPressedThisFrame()
        {
            return _playerInteract.WasPressedThisFrame();
        }

        public void Dispose()
        {
            DisableAllMaps();

            if (_runtimeAsset == null)
            {
                return;
            }

            if (UnityEngine.Application.isPlaying)
            {
                UnityEngine.Object.Destroy(_runtimeAsset);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(_runtimeAsset);
            }
        }

        private InputActionMap FindMap(string mapName)
        {
            return _runtimeAsset.FindActionMap(mapName, throwIfNotFound: false);
        }

        private InputActionMap RequireMap(string mapName)
        {
            return _runtimeAsset.FindActionMap(mapName, throwIfNotFound: true);
        }

        private static InputAction RequireAction(InputActionMap map, string actionName)
        {
            return map.FindAction(actionName, throwIfNotFound: true);
        }

        private static InputActionMap RequireContextMap(InputActionMap map, string mapName)
        {
            if (map == null)
            {
                throw new InvalidOperationException(
                    $"Input action map '{mapName}' is required for this input context.");
            }

            return map;
        }

        private void DisableAllMaps()
        {
            _playerMap.Disable();
            _vehicleMap?.Disable();
            _uiMap?.Disable();
        }
    }
}