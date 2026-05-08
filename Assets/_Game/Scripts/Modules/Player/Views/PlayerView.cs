// [MODULE: Player]
// [TYPE: View]
// [SCOPE: Scene]
// [DEPENDS_ON: none]
// View: chỉ hiển thị / animation. KHÔNG chứa logic game.
using UnityEngine;

namespace BillGameCore.Modules.Player
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public void SetMoving(bool isMoving) => _animator.SetBool("isMoving", isMoving);
        public void SetFlipX(bool flipX) => _spriteRenderer.flipX = flipX;
        public void PlayLevelUpVFX() => _animator.SetTrigger("LevelUp");
    }
}