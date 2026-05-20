using System;
using UnityEngine;

namespace BillGameCore.Modules.Player.Presentation
{
    public sealed class PlayerRuntime : IDisposable
    {
        private readonly PlayerPresenter _presenter;
        private bool _isDisposed;

        public PlayerRuntime(PlayerPresenter presenter)
        {
            _presenter = presenter;
        }

        public void Tick(Vector2 moveInput)
        {
            if (_isDisposed)
            {
                return;
            }

            _presenter.TickMove(moveInput);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _presenter.Stop();
        }
    }
}