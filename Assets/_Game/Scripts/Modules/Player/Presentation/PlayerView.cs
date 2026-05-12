using UnityEngine;

namespace BillGameCore.Modules.Player.Presentation
{
    // Unity-facing visual output for Player.
    // R03: MonoBehaviour callbacks ONLY forward to Presenter — ZERO business logic here.
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private Rigidbody2D  _rb;
        private PlayerPresenter _presenter;

        private void Awake() => _rb = GetComponent<Rigidbody2D>(); // self-GetComponent is allowed

        // Called once by PlayerSpawner immediately after Instantiate.
        public void Bind(PlayerPresenter presenter) => _presenter = presenter;

        // R03: forward only ──────────────────────────────
        private void Update()                         => _presenter?.OnUpdate(Time.deltaTime);
        private void FixedUpdate()                    => _presenter?.OnFixedUpdate();
        private void OnTriggerEnter2D(Collider2D col) => _presenter?.OnTriggerEnter2D(col);

        // View write methods — called by Presenter only ──
        public void SetVelocity(float vx, float vy)
        {
            if (_rb) _rb.linearVelocity = new Vector2(vx, vy);
        }

        public void UpdateMoveAnimation(float dirX, float dirY)
        {
            if (!_animator) return;
            _animator.SetFloat("MoveX", dirX);
            _animator.SetFloat("MoveY", dirY);
            _animator.SetFloat("Speed",  new Vector2(dirX, dirY).sqrMagnitude);
        }

        public void PlayDeath() { if (_animator) _animator.SetTrigger("Die"); }
    }
}