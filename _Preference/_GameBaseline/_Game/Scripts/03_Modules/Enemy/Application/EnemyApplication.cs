using BillGameCore.Core.Combat;
using BillGameCore.Core.Rewards;
using BillGameCore.Modules.Enemy.Domain;
using System;

namespace BillGameCore.Modules.Enemy.Application
{
    public sealed class EnemyApplication : IDamageReceiver
    {
        private readonly EnemyDefinition _definition;
        private readonly EnemyState _state;

        public EnemyApplication(EnemyDefinition definition, EnemyState state)
        {
            _definition = definition;
            _state = state;
        }

        public bool IsDead => _state.IsDead;

        public float CurrentHealth => _state.CurrentHealth;

        public RewardBundle DeathReward => _definition.Reward;
        public void ComputeMoveVelocityToTarget(float currentX, float currentY, float targetX, float targetY, out float velocityX, out float velocityY)
        {
            var directionX = targetX - currentX;
            var directionY = targetY - currentY;
            var magnitudeSquared = (directionX * directionX) + (directionY * directionY);

            var detectionRange = _definition.DetectionRange;
            var detectionRangeSquared = detectionRange * detectionRange;

            if (magnitudeSquared > detectionRangeSquared)
            {
                velocityX = 0f;
                velocityY = 0f;
                return;
            }

            var stopDistance = _definition.StopDistance;
            var stopDistanceSquared = stopDistance * stopDistance;

            if (magnitudeSquared <= stopDistanceSquared)
            {
                velocityX = 0f;
                velocityY = 0f;
                return;
            }

            var magnitude = MathF.Sqrt(magnitudeSquared);
            directionX /= magnitude;
            directionY /= magnitude;

            velocityX = directionX * _definition.MoveSpeed;
            velocityY = directionY * _definition.MoveSpeed;
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