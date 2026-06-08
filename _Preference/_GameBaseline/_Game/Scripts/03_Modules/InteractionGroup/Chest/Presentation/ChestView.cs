using UnityEngine;

namespace BillGameCore.Modules.InteractionGroup.Chest.Presentation
{
    public sealed class ChestView : MonoBehaviour
    {
        [SerializeField] private GameObject _closedVisual;
        [SerializeField] private GameObject _openedVisual;

        public void SetOpened(bool isOpened)
        {
            if (_closedVisual != null)
            {
                _closedVisual.SetActive(!isOpened);
            }

            if (_openedVisual != null)
            {
                _openedVisual.SetActive(isOpened);
            }
        }
    }
}