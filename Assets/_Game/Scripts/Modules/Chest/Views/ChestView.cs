// [MODULE: Chest]
// [TYPE: View]
// [SCOPE: SceneLifetimeScope]
// [DEPENDS_ON: ChestLogic]
// View: MonoBehaviour — animation + visual feedback khi trạng thái thay đổi
// KHÔNG chứa logic game
using VContainer;
using UnityEngine;

namespace BillGameCore.Modules.Chest
{
    public class ChestView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private ChestLogic _logic;
        [SerializeField] private string _persistentId = System.Guid.NewGuid().ToString();

        [Inject]
        public void Construct(ChestLogic logic)
        {
            _logic = logic;
            _logic.OnActivated += HandleActivated;
            _logic.OnReset     += HandleReset;
        }

        private void OnDestroy()
        {
            if (_logic == null) return;
            _logic.OnActivated -= HandleActivated;
            _logic.OnReset     -= HandleReset;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                _logic.Interact(_persistentId);
        }

        private void HandleActivated() { /* TODO: trigger animation */ }
        private void HandleReset()     { /* TODO: trigger reset animation */ }
    }
}