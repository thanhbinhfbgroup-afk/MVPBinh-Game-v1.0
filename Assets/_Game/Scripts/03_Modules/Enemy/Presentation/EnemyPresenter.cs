using System;
using BillGameCore.Core.Combat;
using BillGameCore.Core.Rewards;
using BillGameCore.Modules.Enemy.Application;

namespace BillGameCore.Modules.Enemy.Presentation
{
    public sealed class EnemyPresenter
    {
        private readonly EnemyApplication _application;
        private readonly EnemyView _view;

        public Action<RewardBundle> OnDiedCallback { get; set; }

        public EnemyPresenter(EnemyApplication application, EnemyView view)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public void Initialize()
        {
            if (_application.IsDead)
            {
                _view.Hide();
            }
        }

        public DamageResult ReceiveDamage(DamageInfo damageInfo)
        {
            var result = _application.ReceiveDamage(damageInfo);

            if (result.AppliedDamage > 0f && !result.JustDied)
            {
                _view.ShowHitColor();
            }

            if (result.JustDied)
            {           
                _view.Hide();
                OnDiedCallback?.Invoke(_application.DeathReward);
            }

            return result;
        }
    }
}