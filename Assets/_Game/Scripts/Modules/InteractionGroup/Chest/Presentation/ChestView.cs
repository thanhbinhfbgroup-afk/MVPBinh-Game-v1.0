using UnityEngine;

namespace BillGameCore.Modules.InteractionGroup.Chest.Presentation
{
    // R03: Chỉ drive Animator — không có business logic.
    public sealed class ChestView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        public void PlayActivated()   => _animator?.SetTrigger("Activate");
        public void PlayDeactivated() => _animator?.SetTrigger("Deactivate");
    }
}