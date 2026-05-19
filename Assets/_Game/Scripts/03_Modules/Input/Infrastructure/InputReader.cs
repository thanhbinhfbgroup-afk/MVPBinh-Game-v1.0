using System;
using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.Input.Commands;
using BillGameCore.SharedPorts.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BillGameCore.Modules.Input.Infrastructure
{
    public sealed class InputReader : MonoBehaviour
    {
        private const string DefaultInputAssetPath = "Assets/Settings/InputSystem_Actions.inputactions";

        [SerializeField]
        private InputActionAsset _actions;

        private CommandBuffer _commandBuffer;
        private InputActionGateway _gateway;
        private EntityId _controlledEntityId = EntityId.Invalid;

        [Inject]
        public void Construct(CommandBuffer commandBuffer)
        {
            _commandBuffer = commandBuffer ?? throw new ArgumentNullException(nameof(commandBuffer));

            ValidateConfiguration();

            _gateway = new InputActionGateway(_actions);
            _gateway.EnableContext(InputContext.Player);
        }

        private void OnDisable()
        {
            _gateway?.Dispose();
            _gateway = null;
        }

        private void Update()
        {
            if (_gateway == null)
            {
                return;
            }

            if (!_controlledEntityId.IsValid)
            {
                return;
            }

            ReadPlayerMap();
        }

        public void SetControlledEntity(EntityId controlledEntityId)
        {
            if (!controlledEntityId.IsValid)
            {
                throw new ArgumentException("Controlled entity id must be valid.", nameof(controlledEntityId));
            }

            _controlledEntityId = controlledEntityId;
        }

        public void ValidateConfiguration()
        {
            TryAssignDefaultActionsInEditor();

            if (_actions == null)
            {
                throw new InvalidOperationException($"{nameof(InputReader)} requires an InputActionAsset.");
            }

            var playerMap = _actions.FindActionMap("Player", throwIfNotFound: false);

            if (playerMap == null)
            {
                throw new InvalidOperationException("Input action map 'Player' was not found.");
            }

            ValidateAction(playerMap, "Move");
            ValidateAction(playerMap, "Attack");
            ValidateAction(playerMap, "Interact");
        }

        private void ReadPlayerMap()
        {
            var move = _gateway.ReadMove();
            var command = new MoveCommand(_controlledEntityId, move.x, move.y);

            _commandBuffer.Enqueue(command);
        }

        private static void ValidateAction(InputActionMap map, string actionName)
        {
            if (map.FindAction(actionName, throwIfNotFound: false) == null)
            {
                throw new InvalidOperationException($"Input action '{map.name}/{actionName}' was not found.");
            }
        }

        private void TryAssignDefaultActionsInEditor()
        {
#if UNITY_EDITOR
            if (_actions != null)
            {
                return;
            }

            _actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(DefaultInputAssetPath);
#endif
        }
    }
}