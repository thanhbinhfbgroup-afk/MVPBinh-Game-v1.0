// [MODULE: Input]
// [TYPE: Manager]
// [SCOPE: ProjectLifetimeScope]
// [SIGNAL_PUBLISHES: MoveInputSignal, ActionInputSignal]
// [SIGNAL_SUBSCRIBES: none]
// [DEPENDS_ON: none]
// [REGISTER_IN: ProjectLifetimeScope.cs → Global Services block]
using MessagePipe;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using BillGameCore.Interfaces;
using BillGameCore.Interfaces.Signals;

namespace BillGameCore.Modules.Input
{
    public class InputManager : IInputService, ITickable
    {
        private readonly IPublisher<MoveInputSignal> _movePublisher;
        private readonly IPublisher<ActionInputSignal> _actionPublisher;
        private bool _isEnabled = true;

        public bool IsEnabled => _isEnabled;

        [Inject]
        public InputManager(
            IPublisher<MoveInputSignal> movePublisher,
            IPublisher<ActionInputSignal> actionPublisher)
        {
            _movePublisher = movePublisher;
            _actionPublisher = actionPublisher;
        }

        public void Initialize() => Debug.Log("[InputManager] Initialized.");

        public void Tick()
        {
            if (!_isEnabled) return;

            // Logic đọc phím cơ bản (Có thể nâng cấp lên New Input System sau)
            float x = UnityEngine.Input.GetAxisRaw("Horizontal");
            float y = UnityEngine.Input.GetAxisRaw("Vertical");

            if (x != 0 || y != 0)
            {
                _movePublisher.Publish(new MoveInputSignal(new Vector2(x, y)));
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                _actionPublisher.Publish(new ActionInputSignal("Interact"));
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                _actionPublisher.Publish(new ActionInputSignal("Jump"));
            }
        }

        public void EnableInput() => _isEnabled = true;
        public void DisableInput() => _isEnabled = false;
    }
}