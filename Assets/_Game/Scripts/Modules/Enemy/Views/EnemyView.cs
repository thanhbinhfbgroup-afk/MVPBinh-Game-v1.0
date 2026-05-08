// [MODULE: Enemy]
// [TYPE: View]
// [SCOPE: SceneLifetimeScope]
// [DEPENDS_ON: none]
// View: MonoBehaviour — chỉ animation / VFX / sound cue
// KHÔNG chứa if/else logic game
using UnityEngine;

namespace BillGameCore.Modules.Enemy
{
    public class EnemyView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        // TODO: các method hiển thị — gọi từ Provider qua event/delegate
    }
}