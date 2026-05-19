using System;
using UnityEngine;

namespace BillGameCore.Modules.Player.Presentation
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerView : MonoBehaviour
    {
        private Rigidbody2D _rigidbody;
        private PlayerPresenter _presenter;

        public Vector2 WorldPosition => transform.position;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            _presenter?.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            _presenter = null;
            _rigidbody = null;
        }

        public void Bind(PlayerPresenter presenter)
        {
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        }

        public void SetVelocity(float velocityX, float velocityY)
        {
            if (_rigidbody == null)
            {
                return;
            }

            _rigidbody.linearVelocity = new Vector2(velocityX, velocityY);
        }
    }
}
