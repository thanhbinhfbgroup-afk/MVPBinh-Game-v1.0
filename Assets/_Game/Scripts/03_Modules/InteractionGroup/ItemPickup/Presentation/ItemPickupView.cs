using UnityEngine;

namespace BillGameCore.Modules.InteractionGroup.ItemPickup.Presentation
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ItemPickupView : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private Collider2D _triggerCollider;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _triggerCollider = GetComponent<Collider2D>();
        }

        public void SetCollected(bool isCollected)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = !isCollected;
            }

            if (_triggerCollider != null)
            {
                _triggerCollider.enabled = !isCollected;
            }
        }
    }
}
