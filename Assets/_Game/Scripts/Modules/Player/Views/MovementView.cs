// [MODULE: Player]
// [TYPE: View]
// [SCOPE: SceneLifetimeScope]
// [DEPENDS_ON: none]
using UnityEngine;

namespace BillGameCore.Modules.Player
{
    public class MovementView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        // Không chứa logic game, chỉ render
        public void UpdateVisuals(Vector2 facingDirection)
        {
            if (facingDirection.x != 0)
            {
                _spriteRenderer.flipX = facingDirection.x < 0;
            }
        }
    }
}