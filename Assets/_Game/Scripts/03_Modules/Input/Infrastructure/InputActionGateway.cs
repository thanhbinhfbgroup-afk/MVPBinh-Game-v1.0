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

        private readonly InputActionAsset _runtimeAsset;
        private readonly InputActionMap _playerMap;
        private readonly InputActionMap _vehicleMap;
        private readonly InputActionMap _uiMap;
        private readonly InputAction _playerMove;

        private InputActionMap _currentMap;
        private bool _disposed;

        public InputActionGateway(InputActionAsset sourceAsset)
        {
            if (sourceAsset == null)
            {
                throw new ArgumentNullException(nameof(sourceAsset));
            }

            // Clone runtime để không thao tác trực tiếp trên asset gốc.
            _runtimeAsset = UnityEngine.Object.Instantiate(sourceAsset);

            _playerMap = RequireMap(_runtimeAsset, InputContextNames.Player);
            _vehicleMap = FindMap(_runtimeAsset, InputContextNames.Vehicle);
            _uiMap = FindMap(_runtimeAsset, InputContextNames.UI);

            _playerMove = RequireAction(_playerMap, MoveActionName);

            DisableAllMaps();
            CurrentContext = InputContext.None;
        }

        public InputContext CurrentContext { get; private set; }

        public void SwitchContext(InputContext context)
        {
            ThrowIfDisposed();

            if (context == InputContext.None)
            {
                throw new ArgumentOutOfRangeException(nameof(context), context, "Input context None is not supported.");
            }

            var targetMap = ResolveRequiredMap(context);

            if (_currentMap == targetMap && CurrentContext == context)
            {
                return;
            }

            DisableAllMaps();
            targetMap.Enable();

            _currentMap = targetMap;
            CurrentContext = context;
        }

        public Vector2 ReadPlayerMove()
        {
            ThrowIfDisposed();
            EnsurePlayerContext();

            return _playerMove.ReadValue<Vector2>();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            DisableAllMaps();

            if (UnityEngine.Application.isPlaying)
            {
                UnityEngine.Object.Destroy(_runtimeAsset);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(_runtimeAsset);
            }

            _currentMap = null;
            CurrentContext = InputContext.None;
            _disposed = true;
        }

        private InputActionMap ResolveRequiredMap(InputContext context)
        {
            return context switch
            {
                InputContext.Player => _playerMap,
                InputContext.Vehicle => RequireContextMap(_vehicleMap, InputContextNames.Vehicle),
                InputContext.UI => RequireContextMap(_uiMap, InputContextNames.UI),
                _ => throw new ArgumentOutOfRangeException(nameof(context), context, "Unsupported input context."),
            };
        }

        private static InputActionMap FindMap(InputActionAsset asset, string mapName)
        {
            return asset.FindActionMap(mapName, throwIfNotFound: false);
        }

        private static InputActionMap RequireMap(InputActionAsset asset, string mapName)
        {
            var map = asset.FindActionMap(mapName, throwIfNotFound: false);
            if (map == null)
            {
                throw new InvalidOperationException($"Input action map '{mapName}' was not found.");
            }

            return map;
        }

        private static InputAction RequireAction(InputActionMap map, string actionName)
        {
            var action = map.FindAction(actionName, throwIfNotFound: false);
            if (action == null)
            {
                throw new InvalidOperationException($"Input action '{map.name}/{actionName}' was not found.");
            }

            return action;
        }

        private static InputActionMap RequireContextMap(InputActionMap map, string mapName)
        {
            if (map == null)
            {
                throw new InvalidOperationException($"Input action map '{mapName}' is required for this input context.");
            }

            return map;
        }

        private void DisableAllMaps()
        {
            _playerMap.Disable();
            _vehicleMap?.Disable();
            _uiMap?.Disable();
        }

        private void EnsurePlayerContext()
        {
            if (CurrentContext != InputContext.Player)
            {
                throw new InvalidOperationException($"Current input context must be {InputContext.Player} to read player move, but was {CurrentContext}.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(InputActionGateway));
            }
        }
    }
}