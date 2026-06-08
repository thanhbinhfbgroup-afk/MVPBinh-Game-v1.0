using BillGameCore.Core.Combat;
using BillGameCore.Modules.Player.Domain;
using System;

namespace BillGameCore.Modules.Player.Application
{
    public sealed class PlayerApplication : IDamageReceiver
    {
        private readonly float _moveSpeed;
        private readonly PlayerState _state;

        public bool IsDead => _state.IsDead;

        public float CurrentHealth => _state.CurrentHealth;

        public PlayerApplication(float moveSpeed, PlayerState state)
        {
            _moveSpeed = moveSpeed;
            _state = state;
        }

        public void ComputeMoveVelocity(
            float inputX,
            float inputY,
            out float velocityX,
            out float velocityY)
        {
            var magnitudeSquared = (inputX * inputX) + (inputY * inputY);

            if (magnitudeSquared > 1f)
            {
                var magnitude = MathF.Sqrt(magnitudeSquared);
                inputX /= magnitude;
                inputY /= magnitude;
            }

            velocityX = inputX * _moveSpeed;
            velocityY = inputY * _moveSpeed;
        }
        public DamageResult ReceiveDamage(DamageInfo damageInfo)
        {
            var amount = damageInfo.Amount;
            if (amount < 0f)
            {
                amount = 0f;
            }

            _state.ApplyDamage(amount, out var appliedDamage, out var justDied);

            return new DamageResult(
                appliedDamage,
                _state.CurrentHealth,
                justDied);
        }
    }
}