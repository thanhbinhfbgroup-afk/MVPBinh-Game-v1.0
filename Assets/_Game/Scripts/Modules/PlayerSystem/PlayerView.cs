using UnityEngine;

namespace BillGameCore.Player
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;

        public void Move(Vector2 velocity) => rb.linearVelocity = velocity;

        public void Flip(float horizontalInput)
        {
            if (horizontalInput > 0.1f) spriteRenderer.flipX = false;
            else if (horizontalInput < -0.1f) spriteRenderer.flipX = true;
        }

        public void Animate(Vector2 moveDir)
        {
            if (animator == null) return; // đang test sprite tĩnh, chưa có animator
            animator.SetFloat("Speed", moveDir.magnitude);
        }
    }
}