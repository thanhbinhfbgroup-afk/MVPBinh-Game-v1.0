// [MODULE: Input]
// [TYPE: Provider]
// [SCOPE: SceneLifetimeScope]
// [SIGNAL_PUBLISHES: MoveInputChangedSignal, AttackInputSignal]
// [SIGNAL_SUBSCRIBES: none]
// [DEPENDS_ON: PlayerInput]
// [REGISTER_IN: SceneLifetimeScope.cs -> Scene services block]
using BillGameCore.Interfaces.Signals;
using BillGameCore.Modules.Input.Interfaces;
using MessagePipe;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace BillGameCore.Modules.Input.Providers
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInput))]
    public sealed class InputManager : MonoBehaviour, IInputService
    {
        [SerializeField] private PlayerInput _playerInput;

        private IPublisher<MoveInputChangedSignal> _moveInputPublisher;
        private IPublisher<AttackInputSignal> _attackInputPublisher;

        private InputAction _moveAction;
        private InputAction _attackAction;
        private InputAction _interactAction;
        private Vector2 _moveDirection;

        [Inject]
        public void Construct(
            IPublisher<MoveInputChangedSignal> moveInputPublisher,
            IPublisher<AttackInputSignal> attackInputPublisher)
        {
            _moveInputPublisher = moveInputPublisher;
            _attackInputPublisher = attackInputPublisher;
        }

        public void Initialize()
        {
        }

        public Vector2 GetMoveDirection() => _moveDirection;
        public bool IsAttackPressed() => _attackAction != null && _attackAction.IsPressed();
        public bool IsInteractPressed() => _interactAction != null && _interactAction.IsPressed();

        private void Awake()
        {
            if (_playerInput == null)
            {
                _playerInput = GetComponent<PlayerInput>();
            }

            var actions = _playerInput != null ? _playerInput.actions : null;
            _moveAction = actions?.FindAction("Move");
            _attackAction = actions?.FindAction("Attack");
            _interactAction = actions?.FindAction("Interact");

            if (_moveAction == null || _attackAction == null || _interactAction == null)
            {
                Debug.LogError("[InputManager] Missing Move / Attack / Interact actions.");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_moveAction == null || _attackAction == null)
            {
                return;
            }

            _moveAction.performed += OnMoveChanged;
            _moveAction.canceled += OnMoveChanged;
            _attackAction.performed += OnAttackPerformed;
        }

        private void OnDisable()
        {
            if (_moveAction != null)
            {
                _moveAction.performed -= OnMoveChanged;
                _moveAction.canceled -= OnMoveChanged;
            }

            if (_attackAction != null)
            {
                _attackAction.performed -= OnAttackPerformed;
            }
        }

        private void OnMoveChanged(InputAction.CallbackContext context)
        {
            var nextDirection = context.ReadValue<Vector2>();
            if (nextDirection == _moveDirection)
            {
                return;
            }

            _moveDirection = nextDirection;
            _moveInputPublisher.Publish(new MoveInputChangedSignal(_moveDirection));
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            _attackInputPublisher.Publish(new AttackInputSignal());
        }
    }
}
