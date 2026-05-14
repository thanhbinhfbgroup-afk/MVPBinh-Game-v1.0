using System;
using BillGameCore.Modules.Input.Context;
using BillGameCore.SharedPorts.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BillGameCore.Modules.Input.Infrastructure
{
    // Owns a runtime clone of InputActionAsset and exposes typed reads to InputReader.
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
                throw new InvalidOperationException($"{nameof(InputActionGateway)} requires an InputActionAsset.");

            _runtimeAsset = UnityEngine.Object.Instantiate(sourceAsset);
            _playerMap = RequireMap(PlayerInputContext.ActionMapName);
            _vehicleMap = FindMap(VehicleInputContext.ActionMapName);
            _uiMap = FindMap(UIInputContext.ActionMapName);

            _playerMove = RequireAction(_playerMap, MoveActionName);
            _playerAttack = RequireAction(_playerMap, AttackActionName);
            _playerInteract = RequireAction(_playerMap, InteractActionName);
        }

        public void SwitchContext(InputContext context)
        {
            DisableAllMaps();

            switch (context)
            {
                case InputContext.Player:
                    _playerMap.Enable();
                    break;
                case InputContext.Vehicle:
                    RequireContextMap(_vehicleMap, VehicleInputContext.ActionMapName).Enable();
                    break;
                case InputContext.UI:
                    RequireContextMap(_uiMap, UIInputContext.ActionMapName).Enable();
                    break;
                case InputContext.None:
                    break;
                default:
                    _playerMap.Enable();
                    break;
            }
        }

        public Vector2 ReadPlayerMove() => _playerMove.ReadValue<Vector2>();

        public bool WasPlayerAttackPressedThisFrame() => _playerAttack.WasPressedThisFrame();

        public bool WasPlayerAttackReleasedThisFrame() => _playerAttack.WasReleasedThisFrame();

        public bool WasPlayerInteractPressedThisFrame() => _playerInteract.WasPressedThisFrame();

        public void Dispose()
        {
            DisableAllMaps();
            if (_runtimeAsset != null)
            {
                if (UnityEngine.Application.isPlaying)
                    UnityEngine.Object.Destroy(_runtimeAsset);
                else
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
                throw new InvalidOperationException($"Input action map '{mapName}' is required for this input context.");

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
