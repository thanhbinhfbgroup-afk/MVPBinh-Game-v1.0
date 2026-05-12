
using BillGameCore.Modules.Input.Commands;
using BillGameCore.Modules.Input.Context;
using BillGameCore.SharedPorts.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;

namespace BillGameCore.Modules.Input.Infrastructure
{
    // MonoBehaviour — translates New Input System events into ICommand objects.
    // R03: No business logic — only raw-input → Command translation.
    // R16: This is the ONLY class allowed to call CommandBuffer.Enqueue().
    // R17: Registered via builder.RegisterComponent<InputReader>() — VContainer resolves [Inject].
    public sealed class InputReader : MonoBehaviour
    {
        [Inject] private CommandBuffer _buffer;            // injected by VContainer

        [Inject] private PlayerInput _playerInput; 

        private EntityId     _controlledEntityId = EntityId.Invalid;
        private InputContext _currentContext      = InputContext.Player;

        // Hold-state for Attack
        private bool  _attackHeld;
        private float _attackHeldStart;

        // Called by GameBootstrapper after PlayerSpawner.Spawn() returns a Runtime.
        public void SetControlledEntity(EntityId id) => _controlledEntityId = id;

        public void SwitchContext(InputContext context)
        {
            _currentContext = context;
            _buffer.Clear(); // flush stale commands (CONTEXT 14E)
            _playerInput.SwitchCurrentActionMap(context switch
            {
                InputContext.Player  => PlayerInputContext.ActionMapName,
                InputContext.Vehicle => VehicleInputContext.ActionMapName,
                InputContext.UI      => UIInputContext.ActionMapName,
                _                   => PlayerInputContext.ActionMapName,
            });
        }

        private void Update()
        {
            if (!_controlledEntityId.IsValid) return;
            switch (_currentContext)
            {
                case InputContext.Player:  ReadPlayerMap();  break;
                case InputContext.Vehicle: ReadVehicleMap(); break;
            }
        }

        // R16: All Enqueue calls are inside this file only.
        private void ReadPlayerMap()
        {
            var mv = _playerInput.actions["Player/Move"].ReadValue<Vector2>();
            _buffer.Enqueue(new MoveCommand(_controlledEntityId, mv.x, mv.y, Time.time));

            var atk = _playerInput.actions["Player/Attack"];
            if (atk.WasPressedThisFrame()) { _attackHeld = true; _attackHeldStart = Time.time; }
            if (_attackHeld)
                _buffer.Enqueue(new AttackCommand(_controlledEntityId, Time.time,
                                                  isHeld: true,
                                                  heldDuration: Time.time - _attackHeldStart));
            if (atk.WasReleasedThisFrame()) _attackHeld = false;

            if (_playerInput.actions["Player/Interact"].WasPressedThisFrame())
                _buffer.Enqueue(new InteractCommand(_controlledEntityId, Time.time));
        }

        private void ReadVehicleMap()
        {
            // Implement Vehicle action reads here when Vehicle slice is built.
        }
    }
}