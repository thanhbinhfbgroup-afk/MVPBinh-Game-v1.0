using BillGameCore.Core.Combat;
using UnityEngine;

namespace BillGameCore.Modules.Enemy.Presentation
{
    public sealed class EnemyView : MonoBehaviour, IDamageReceiver
    {
        private EnemyPresenter _presenter;

        public void Bind(EnemyPresenter presenter)
        {
            _presenter = presenter;
        }

        public DamageResult ReceiveDamage(DamageInfo damageInfo)
        {
            return _presenter.ReceiveDamage(damageInfo);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}