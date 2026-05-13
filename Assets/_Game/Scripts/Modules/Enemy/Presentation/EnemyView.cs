using UnityEngine;

namespace BillGameCore.Modules.Enemy.Presentation
{
    // Output visual phía Unity cho Enemy.
    // R03: MonoBehaviour callback CHỈ forward sang Presenter — KHÔNG có business logic nào ở đây.
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private Rigidbody2D  _rb;
        private EnemyPresenter _presenter;

        private void Awake() => _rb = GetComponent<Rigidbody2D>(); // GetComponent trên self được phép

        // Gọi một lần bởi EnemySpawner ngay sau Instantiate.
        public void Bind(EnemyPresenter presenter) => _presenter = presenter;

        // R03: chỉ forward ──────────────────────────────────
        private void Update()                         => _presenter?.OnUpdate(Time.deltaTime);
        private void FixedUpdate()                    => _presenter?.OnFixedUpdate();
        private void OnTriggerEnter2D(Collider2D col) => _presenter?.OnTriggerEnter2D(col);

        // Các method write của View — chỉ được gọi bởi Presenter ──────
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

        // Vị trí thế giới thực — Presenter dùng khi emit OnDied (FIX-05).
        public Vector2 WorldPosition => transform.position;
    }
}