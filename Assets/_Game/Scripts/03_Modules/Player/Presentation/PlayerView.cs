using UnityEngine;
using System;

namespace BillGameCore.Modules.Player.Presentation
{
    public sealed class PlayerView : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;
        public event Action<Collider2D> TriggerEntered;
        public event Action<Collider2D> TriggerExited;

        public Vector2 WorldPosition => _rigidbody2D.position;

        public void SetMoveVelocity(Vector2 velocity)
        {
            if (_rigidbody2D == null)
            {
                return;
            }

            _rigidbody2D.linearVelocity = velocity;
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            TriggerEntered?.Invoke(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            TriggerExited?.Invoke(other);
        }
    }
}
