using System;
using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.Input.Commands;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BillGameCore.Modules.Input.Infrastructure
{
    public sealed class InputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _actions;

        private InputActionGateway _inputActionGateway;
        private CommandBuffer _commandBuffer;
        private BillEntityId _controlledEntityId;

        public void SetControlledEntity(BillEntityId controlledEntityId)
        {
            if (!controlledEntityId.IsValid)
            {
                throw new InvalidOperationException("InputReader requires a valid controlled entity id.");
            }

            _controlledEntityId = controlledEntityId;
        }
       
        public void SetCommandBuffer(CommandBuffer commandBuffer)
        {
            _commandBuffer = commandBuffer;
        }

        private void Update()
        {
            ReadPlayerMap();
        }

        private void ReadPlayerMap()
        {
            if (_commandBuffer == null)
            {
                throw new InvalidOperationException("InputReader requires a CommandBuffer before Update runs.");
            }

            if (!_controlledEntityId.IsValid)
            {
                throw new InvalidOperationException("InputReader requires a controlled entity id before Update runs.");
            }

            var moveInput = _inputActionGateway.ReadMoveInput();
            var dirX = moveInput.x;
            var dirY = moveInput.y;

            _commandBuffer.Enqueue(new MoveCommand(_controlledEntityId, dirX, dirY));

            if (_inputActionGateway.WasAttackPerformedThisFrame())
            {
                _commandBuffer.Enqueue(new AttackCommand(_controlledEntityId, isPerformed: true));
            }

            if (_inputActionGateway.WasInteractPerformedThisFrame())
            {
                _commandBuffer.Enqueue(new InteractCommand(_controlledEntityId, isPerformed: true));
            }
        }

        public void ConsumeSwitchContext(SwitchContextCommand command)
        {
            if (command == null)
            {
                throw new InvalidOperationException("SwitchContextCommand cannot be null.");
            }

            if (command.ControlledEntityId != _controlledEntityId)
            {
                return;
            }

            _inputActionGateway.SwitchContext(command.TargetContext);
        }

        private void Awake()
        {
            _inputActionGateway = new InputActionGateway(_actions);
        }

        private void OnEnable()
        {
            _inputActionGateway?.EnablePlayerMap();
        }

        private void OnDisable()
        {
            _inputActionGateway?.DisablePlayerMap();
        }

        private void OnDestroy()
        {
            _inputActionGateway?.Dispose();
        }

        public InputActionGateway ValidateConfiguration()
        {
            if (_actions == null)
            {
                throw new InvalidOperationException("InputReader requires an InputActionAsset.");
            }

            return new InputActionGateway(_actions);
        }
    }
}
