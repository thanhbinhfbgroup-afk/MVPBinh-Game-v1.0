using BillGameCore.Core.Combat;
using System.Collections;
using UnityEngine;

namespace BillGameCore.Modules.Enemy.Presentation
{
    public sealed class EnemyView : MonoBehaviour, IDamageReceiver
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _hitColor = Color.red;
        [SerializeField] private float _hitFlashDuration = 0.1f;

        private EnemyPresenter _presenter;
        private Color _defaultColor;
        private Coroutine _hitFlashRoutine;

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
    }
}