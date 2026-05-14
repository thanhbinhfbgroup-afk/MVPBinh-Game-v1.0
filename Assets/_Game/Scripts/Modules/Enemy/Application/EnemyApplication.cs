using System;
using BillGameCore.Core.Combat;
using BillGameCore.Core.Rewards;
using BillGameCore.Modules.Enemy.Domain;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;

namespace BillGameCore.Modules.Enemy.Application
{
    // Logic use-case của Enemy. Không dùng UnityEngine và không expose Player read contract.
    public sealed class EnemyApplication : IDamageReceiver
    {
        private readonly EnemyDefinition _def;
        private readonly EnemyState      _state;

        public event Action<EntityId, RewardBundle> OnDied;

        public EntityId Id { get; }

        public EnemyApplication(EntityId id, EnemyDefinition def, EnemyState state)
        {
            Id = id;
            _def = def;
            _state = state;
            _state.CurrentHealth = def.MaxHealth;
            _state.CurrentStamina = def.MaxStamina;
        }

        public float VelocityX => _state.VelocityX;
        public float VelocityY => _state.VelocityY;
        public bool  IsDead    => _state.IsDead;

        public void Tick(float dirX, float dirY, float deltaTime)
        {
            if (_state.IsDead) return;

            _state.VelocityX = dirX * _def.MoveSpeed;
            _state.VelocityY = dirY * _def.MoveSpeed;
        }

        public DamageResult ReceiveDamage(DamageInfo damage)
        {
            if (_state.IsDead) return new DamageResult(0f, 0f, false);

            float applied = System.Math.Min(damage.Amount, _state.CurrentHealth);
            _state.CurrentHealth -= applied;

            bool justDied = _state.CurrentHealth <= 0f;
            if (justDied)
            {
                _state.IsDead = true;
                OnDied?.Invoke(Id, CreateRewardBundle());
            }

            return new DamageResult(applied, _state.CurrentHealth, justDied);
        }

        private RewardBundle CreateRewardBundle()
        {
            return new RewardBundle
            {
                Gold = _def.GoldReward,
                Experience = _def.ExperienceReward,
            };
        }
    }
}
