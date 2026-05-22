using System;
using BillGameCore.Modules.Input.Commands;
using BillGameCore.Core.ValueObjects;
using UnityEngine;
using UnityEngine.InputSystem;
using BillGameCore.Modules.Input.Context;

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
        }

        private void Awake()
        {
            ValidateConfiguration();
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

        public void ValidateConfiguration()
        {
            if (_actions == null)
            {
                throw new InvalidOperationException("InputReader requires an InputActionAsset.");
            }

            var playerActionMap = _actions.FindActionMap(InputContextNames.Player, throwIfNotFound: false);
            if (playerActionMap == null)
            {
                throw new InvalidOperationException("InputReader could not find action map 'Player'.");
            }

            var moveAction = playerActionMap.FindAction("Move", throwIfNotFound: false);
            if (moveAction == null)
            {
                throw new InvalidOperationException("InputReader could not find action 'Player/Move'.");
            }
        }
    }
}