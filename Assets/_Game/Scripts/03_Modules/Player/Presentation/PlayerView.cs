using BillGameCore.Core.Combat;
using UnityEngine;
using System;

namespace BillGameCore.Modules.Player.Presentation
{
    public sealed class PlayerView : MonoBehaviour, IDamageReceiver
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;
        private PlayerPresenter _presenter;
        public event Action<Collider2D> TriggerEntered;
        public event Action<Collider2D> TriggerExited;

        public Vector2 WorldPosition => _rigidbody2D.position;

        public void Bind(PlayerPresenter presenter)
        {
            _presenter = presenter;
        }

        public DamageResult ReceiveDamage(DamageInfo damageInfo)
        {
            return _presenter.ReceiveDamage(damageInfo);
        }

        public void SetMoveVelocity(Vector2 velocity)
        {
            if (_rigidbody2D == null)
            {
                return;
            }

            _rigidbody2D.linearVelocity = velocity;
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            TriggerEntered?.Invoke(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            TriggerExited?.Invoke(other);
        }
    }
}
