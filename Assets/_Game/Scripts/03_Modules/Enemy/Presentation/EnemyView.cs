using BillGameCore.Core.Combat;
using BillGameCore.Core.ValueObjects;
using System.Collections;
using UnityEngine;

namespace BillGameCore.Modules.Enemy.Presentation
{
    public sealed class EnemyView : MonoBehaviour, IDamageReceiver
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _hitColor = Color.red;
        [SerializeField] private float _hitFlashDuration = 0.1f;
        [SerializeField] private float _contactDamageInterval = 5f;

        private EnemyPresenter _presenter;
        private Color _defaultColor;
        private Coroutine _hitFlashRoutine;
        private IDamageReceiver _currentTargetDamageReceiver;
        private Collider2D _currentTargetCollider;
        private float _contactDamageCooldown;
        private BillEntityId _contactDamageSourceId;
        private float _contactDamageAmount;
        public bool CanDamageCurrentTarget => _contactDamageCooldown <= 0f;

        public void SetContactDamage(BillEntityId sourceId, float damageAmount)
        {
            _contactDamageSourceId = sourceId;
            _contactDamageAmount = damageAmount;
        }
        public BillEntityId ContactDamageSourceId => _contactDamageSourceId;
        public float ContactDamageAmount => _contactDamageAmount;
        public IDamageReceiver CurrentTargetDamageReceiver => _currentTargetDamageReceiver;

        private void Update()
        {
            if (_contactDamageCooldown > 0f)
            {
                _contactDamageCooldown -= Time.deltaTime;
            }

            if (_currentTargetDamageReceiver == null)
            {
                return;
            }

            if (!CanDamageCurrentTarget)
            {
                return;
            }

            var damageInfo = new DamageInfo(_contactDamageAmount, _contactDamageSourceId, false);
            if (!TryDamageCurrentTarget(damageInfo, out var damageResult))
            {
                return;
            }

            if (damageResult.AppliedDamage <= 0f)
            {
                return;
            }

            ResetContactDamageCooldown();
        }

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                throw new MissingComponentException(
                    $"{nameof(EnemyView)} on '{gameObject.name}' requires a {nameof(SpriteRenderer)} reference.");
            }

            _defaultColor = _spriteRenderer.color;
        }

        public void Bind(EnemyPresenter presenter)
        {
            _presenter = presenter;
        }

        public DamageResult ReceiveDamage(DamageInfo damageInfo)
        {
            return _presenter.ReceiveDamage(damageInfo);
        }

        public void ShowHitColor()
        {
            if (_hitFlashRoutine != null)
            {
                StopCoroutine(_hitFlashRoutine);
            }

            _hitFlashRoutine = StartCoroutine(HitFlashRoutine());
        }

        private IEnumerator HitFlashRoutine()
        {
            _spriteRenderer.color = _hitColor;
            yield return new WaitForSeconds(_hitFlashDuration);
            RestoreDefaultColor();
            _hitFlashRoutine = null;
        }

        public void RestoreDefaultColor()
        {
            _spriteRenderer.color = _defaultColor;
        }

        public void Hide()
        {
            if (_hitFlashRoutine != null)
            {
                StopCoroutine(_hitFlashRoutine);
                _hitFlashRoutine = null;
            }

            RestoreDefaultColor();
            gameObject.SetActive(false);
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null)
            {
                return;
            }

            var damageReceiver = other.GetComponent<IDamageReceiver>();
            if (damageReceiver == null)
            {
                return;
            }

            _currentTargetCollider = other;
            _currentTargetDamageReceiver = damageReceiver;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other == null)
            {
                return;
            }

            if (ReferenceEquals(other, _currentTargetCollider))
            {
                _currentTargetCollider = null;
                _currentTargetDamageReceiver = null;
            }
        }
        public bool TryDamageCurrentTarget(DamageInfo damageInfo, out DamageResult damageResult)
        {
            if (_currentTargetDamageReceiver == null)
            {
                damageResult = default;
                return false;
            }

            damageResult = _currentTargetDamageReceiver.ReceiveDamage(damageInfo);
            return true;
        }
        public void ResetContactDamageCooldown()
        {
            _contactDamageCooldown = _contactDamageInterval;
        }
    }
}