using UnityEngine;

namespace BillGameCore.Modules.Player.Presentation
{
    public sealed class PlayerView : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;

        public Vector2 WorldPosition => _rigidbody2D.position;

        public void SetMoveVelocity(Vector2 velocity)
        {
            _rigidbody2D.linearVelocity = velocity;
        }
    }
}