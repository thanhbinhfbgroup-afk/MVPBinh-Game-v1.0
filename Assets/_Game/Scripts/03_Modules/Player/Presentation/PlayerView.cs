using BillGameCore.Core.Combat;
using BillGameCore.Core.Interaction;
using BillGameCore.Modules.Player.Infrastructure.Config;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BillGameCore.Modules.Player.Presentation
{
    public sealed class PlayerView : MonoBehaviour, IDamageReceiver
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private AttackSensor _attackSensor;
        [SerializeField] private InteractSensor _interactSensor;
        [SerializeField] private float _attackSensorForwardOffset = 0.1f;
        [SerializeField] private float _interactSensorForwardOffset = 0.12f;
        private PlayerPresenter _presenter;

        public Vector2 WorldPosition => _rigidbody2D.position;
        public void Configure(PlayerConfig config)
        {
            if (config == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerView)} on '{gameObject.name}' requires a {nameof(PlayerConfig)}.");
            }

            if (_attackSensor == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerView)} on '{gameObject.name}' requires an {nameof(AttackSensor)} reference.");
            }

            if (_interactSensor == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerView)} on '{gameObject.name}' requires an {nameof(InteractSensor)} reference.");
            }

            var attackCollider = _attackSensor.GetComponent<CircleCollider2D>();
            if (attackCollider == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(AttackSensor)} on '{_attackSensor.gameObject.name}' requires a {nameof(CircleCollider2D)}.");
            }

            var interactCollider = _interactSensor.GetComponent<CircleCollider2D>();
            if (interactCollider == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(InteractSensor)} on '{_interactSensor.gameObject.name}' requires a {nameof(CircleCollider2D)}.");
            }

            _attackSensorForwardOffset = config.AttackForwardOffset;
            _interactSensorForwardOffset = config.InteractForwardOffset;
            attackCollider.radius = config.AttackRadius;
            interactCollider.radius = config.InteractRadius;
        }
        public void SetSensorFacingDirection(Vector2 direction)
        {
            if (direction == Vector2.zero)
            {
                return;
            }

            if (_attackSensor == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerView)} on '{gameObject.name}' requires an {nameof(AttackSensor)} reference.");
            }

            if (_interactSensor == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerView)} on '{gameObject.name}' requires an {nameof(InteractSensor)} reference.");
            }

            var attackLocalPosition = _attackSensor.transform.localPosition;
            attackLocalPosition.x = direction.x * _attackSensorForwardOffset;
            attackLocalPosition.y = direction.y * _attackSensorForwardOffset;
            _attackSensor.transform.localPosition = attackLocalPosition;

            var interactLocalPosition = _interactSensor.transform.localPosition;
            interactLocalPosition.x = direction.x * _interactSensorForwardOffset;
            interactLocalPosition.y = direction.y * _interactSensorForwardOffset;
            _interactSensor.transform.localPosition = interactLocalPosition;
        }
        public bool TryGetNearestAttackTarget(out Collider2D targetCollider, out IDamageReceiver damageReceiver)
        {
            if (_attackSensor == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerView)} on '{gameObject.name}' requires an {nameof(AttackSensor)} reference.");
            }

            return _attackSensor.TryGetNearestTarget(out targetCollider, out damageReceiver);
        }
        public bool TryGetNearestInteractTarget(out Collider2D targetCollider, out IInteractable interactable)
        {
            if (_interactSensor == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerView)} on '{gameObject.name}' requires an {nameof(InteractSensor)} reference.");
            }

            return _interactSensor.TryGetNearestTarget(out targetCollider, out interactable);
        }
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
    }
}
