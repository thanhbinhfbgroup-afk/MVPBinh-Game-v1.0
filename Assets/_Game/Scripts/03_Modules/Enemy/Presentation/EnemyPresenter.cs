using System;
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
            _view.SetDead(_application.IsDead);
        }

        public bool CanInteract()
        {
            return _application.CanInteract();
        }

        public EnemyDeathResult TryInteractKill()
        {
            var result = _application.TryKill();

            if (result.JustDied)
            {
                _view.SetDead(true);
                OnDiedCallback?.Invoke(result.Reward);
            }

            return result;
        }
    }
}