using BillGameCore.Modules.Input.Commands;
using BillGameCore.SharedPorts.Input;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;

namespace BillGameCore.Modules.Input.Infrastructure
{
    // MonoBehaviour adapter: raw Unity input -> command objects.
    // R16: This is the only class allowed to enqueue commands.
    public sealed class InputReader : MonoBehaviour
    {
#if UNITY_EDITOR
        // Editor-only safety net for the approved baseline scene.
        // Runtime ownership is still the serialized _actions field; when multiple
        // scenes/input assets exist, assign _actions explicitly per scene.
        private const string DefaultActionsAssetPath = "Assets/Settings/InputSystem_Actions.inputactions";
#endif

        [Inject] private CommandBuffer _buffer;

        [SerializeField] private InputActionAsset _actions;

        private EntityId _controlledEntityId = EntityId.Invalid;
        private InputContext _currentContext = InputContext.Player;
        private InputActionGateway _gateway;

        private bool _attackHeld;
        private float _attackHeldStart;

        public void SetControlledEntity(EntityId id)
        {
            if (!id.IsValid)
                throw new ArgumentException("Controlled entity id must be valid.", nameof(id));

            _controlledEntityId = id;
        }

        public void ValidateConfiguration()
        {
            TryAssignDefaultActionsAssetInEditor();

            if (_actions == null)
                throw new InvalidOperationException($"{nameof(InputReader)} requires an InputActionAsset.");

            using (new InputActionGateway(_actions)) { }
        }

        public void SwitchContext(InputContext context)
        {
            if (_buffer == null)
                throw new InvalidOperationException($"{nameof(InputReader)} requires {nameof(CommandBuffer)} injection before switching input context.");

            _attackHeld = false;
            _buffer.Clear();
            EnsureGateway();
            _gateway.SwitchContext(context);
            _currentContext = context;
        }

        private void Awake()
        {
            TryAssignDefaultActionsAssetInEditor();
            EnsureGateway();
            _gateway.SwitchContext(_currentContext);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            TryAssignDefaultActionsAssetInEditor();
        }

        private void OnValidate()
        {
            TryAssignDefaultActionsAssetInEditor();
        }
#endif

        private void Update()
        {
            if (!_controlledEntityId.IsValid) return;
            if (_buffer == null) return;

            EnsureGateway();
            switch (_currentContext)
            {
                case InputContext.Player:
                    ReadPlayerMap();
                    break;
                case InputContext.Vehicle:
                    ReadVehicleMap();
                    break;
            }
        }

        private void OnDestroy()
        {
            _gateway?.Dispose();
            _gateway = null;
        }

        private void ReadPlayerMap()
        {
            var move = _gateway.ReadPlayerMove();
            _buffer.Enqueue(new MoveCommand(_controlledEntityId, Time.time, move.x, move.y));

            if (_gateway.WasPlayerAttackPressedThisFrame())
            {
                _attackHeld = true;
                _attackHeldStart = Time.time;
            }

            if (_attackHeld)
            {
                _buffer.Enqueue(new AttackCommand(_controlledEntityId, Time.time,
                                                  isHeld: true,
                                                  heldDuration: Time.time - _attackHeldStart));
            }

            if (_gateway.WasPlayerAttackReleasedThisFrame())
                _attackHeld = false;

            if (_gateway.WasPlayerInteractPressedThisFrame())
                _buffer.Enqueue(new InteractCommand(_controlledEntityId, Time.time));
        }

        private void ReadVehicleMap()
        {
            // Vehicle commands are added when the Vehicle slice exists.
        }

        private void EnsureGateway()
        {
            _gateway ??= new InputActionGateway(_actions);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void TryAssignDefaultActionsAssetInEditor()
        {
#if UNITY_EDITOR
            if (_actions != null)
                return;

            // Do not treat this as a service locator pattern for runtime.
            // It only prevents editor Play Mode from failing after scene/script refresh.
            _actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(DefaultActionsAssetPath);
            if (_actions != null && !UnityEngine.Application.isPlaying)
                EditorUtility.SetDirty(this);
#endif
        }
    }
}
