using UnityEngine;
using BillGameCore.Core.Interaction;

namespace BillGameCore.Modules.Enemy.Presentation
{
    public sealed class EnemyView : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject _aliveVisual;
        [SerializeField] private GameObject _deadVisual;
        private EnemyPresenter _presenter;

        public void Bind(EnemyPresenter presenter)
        {
            _presenter = presenter;
        }

        public void SetDead(bool isDead)
        {
            if (_aliveVisual != null)
            {
                _aliveVisual.SetActive(!isDead);
            }

            if (_deadVisual != null)
            {
                _deadVisual.SetActive(isDead);
            }
        }
        public bool CanInteract()
        {
            return _presenter != null && _presenter.CanInteract();
        }
        public void Interact()
        {
            _presenter?.TryInteractKill();
        }
    }
}