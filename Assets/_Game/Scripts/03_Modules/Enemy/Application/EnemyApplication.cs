using BillGameCore.Core.Combat;
using BillGameCore.Core.Rewards;
using BillGameCore.Modules.Enemy.Domain;

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