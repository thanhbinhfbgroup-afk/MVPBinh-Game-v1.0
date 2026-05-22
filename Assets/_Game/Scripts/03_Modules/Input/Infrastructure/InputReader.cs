using System;
using BillGameCore.Modules.Input.Commands;
using BillGameCore.Core.ValueObjects;
using UnityEngine;
using UnityEngine.InputSystem;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;

namespace BillGameCore.Modules.Input.Infrastructure
{
    public sealed class InputReader : MonoBehaviour
    {
        private const string PlayerActionMapName = "Player";
        private const string MoveActionName = "Move";

        [SerializeField] private InputActionAsset _actions;

        private InputActionMap _playerActionMap;
        private InputAction _moveAction;
        private CommandBuffer _commandBuffer;
        private EntityId _controlledEntityId;

        public void SetControlledEntity(EntityId controlledEntityId)
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
            if (_commandBuffer == null)
            {
                throw new InvalidOperationException("InputReader requires a CommandBuffer before Update runs.");
            }

            if (!_controlledEntityId.IsValid)
            {
                throw new InvalidOperationException("InputReader requires a valid controlled entity before Update runs.");
            }

            var moveInput = _moveAction.ReadValue<Vector2>();
            _commandBuffer.Enqueue(new MoveCommand(_controlledEntityId, moveInput.x, moveInput.y));
        }

        private void Awake()
        {
            ValidateConfiguration();
            CacheActions();
        }

        private void OnEnable()
        {
            _playerActionMap.Enable();
        }

        private void OnDisable()
        {
            _playerActionMap.Disable();
        }

        public void ValidateConfiguration()
        {
            if (_actions == null)
            {
                throw new InvalidOperationException("InputReader requires an InputActionAsset.");
            }

            var playerActionMap = _actions.FindActionMap(PlayerActionMapName, throwIfNotFound: false);
            if (playerActionMap == null)
            {
                throw new InvalidOperationException("InputReader could not find action map 'Player'.");
            }

            var moveAction = playerActionMap.FindAction(MoveActionName, throwIfNotFound: false);
            if (moveAction == null)
            {
                throw new InvalidOperationException("InputReader could not find action 'Player/Move'.");
            }
        }

        private void CacheActions()
        {
            _playerActionMap = _actions.FindActionMap(PlayerActionMapName, throwIfNotFound: true);
            _moveAction = _playerActionMap.FindAction(MoveActionName, throwIfNotFound: true);
        }
    }
}