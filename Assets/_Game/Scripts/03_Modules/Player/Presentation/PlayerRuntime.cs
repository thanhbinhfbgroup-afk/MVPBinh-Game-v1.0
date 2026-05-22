using System;

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

        public void Tick()
        {
            if (_isDisposed)
            {
                return;
            }

            _presenter.Tick();
        }

        public void SetOnDiedCallback(Action onDiedCallback)
        {
            _presenter.OnDiedCallback = onDiedCallback;
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