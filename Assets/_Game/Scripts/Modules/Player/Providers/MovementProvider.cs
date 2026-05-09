// [MODULE: Player]
// [TYPE: Provider]
// [SCOPE: SceneLifetimeScope]
// [DEPENDS_ON: MovementLogic]
using UnityEngine;
using VContainer;

namespace BillGameCore.Modules.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovementProvider : MonoBehaviour
    {
        private MovementLogic _logic;
        private MovementView _view;
        private Rigidbody2D _rb2d;

        [Inject]
        public void Construct(MovementLogic logic)
        {
            _logic = logic;
        }

        private void Awake()
        {
            _rb2d = GetComponent<Rigidbody2D>();
            _view = GetComponent<MovementView>();
        }

        private void FixedUpdate()
        {
            if (_logic == null) return;

            // Unity 6.4 ưu tiên dùng linearVelocity thay vì velocity
            _rb2d.linearVelocity = _logic.CurrentVelocity;

            // Cập nhật View
            if (_logic.CurrentVelocity.sqrMagnitude > 0.01f)
            {
                _view.UpdateVisuals(_logic.CurrentVelocity);
            }
        }
    }
}