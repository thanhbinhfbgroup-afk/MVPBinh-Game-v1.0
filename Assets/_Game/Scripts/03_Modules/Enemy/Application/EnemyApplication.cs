using BillGameCore.Core.Rewards;
using BillGameCore.Modules.Enemy.Domain;

namespace BillGameCore.Modules.Enemy.Application
{
    public sealed class EnemyApplication
    {
        private readonly EnemyDefinition _definition;
        private readonly EnemyState _state;

        public EnemyApplication(EnemyDefinition definition, EnemyState state)
        {
            _definition = definition;
            _state = state;
        }

        public bool IsDead => _state.IsDead;

        public bool CanInteract()
        {
            return !_state.IsDead;
        }

        public EnemyDeathResult TryKill()
        {
            if (!_state.TryMarkDead())
            {
                return new EnemyDeathResult(false, new RewardBundle(0, 0, 0));
            }

            return new EnemyDeathResult(true, _definition.Reward);
        }
    }
}