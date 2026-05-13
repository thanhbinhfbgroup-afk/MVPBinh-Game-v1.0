using System;
using BillGameCore.Modules.InteractionGroup.Chest.Application;

namespace BillGameCore.Modules.InteractionGroup.Chest.Presentation
{
    // Bridge ChestApplication event → ChestView animation.
    // Được tạo bởi ChestBinder trong Awake.
    public sealed class ChestPresenter : IDisposable
    {
        private readonly ChestApplication _app;
        private readonly ChestView        _view;

        public ChestPresenter(ChestApplication app, ChestView view)
        {
            _app  = app;
            _view = view;
            _app.Activated   += _view.PlayActivated;
            _app.Deactivated += _view.PlayDeactivated;
        }

        public bool TryActivate() => _app.TryActivate();

        public void Dispose()
        {
            _app.Activated   -= _view.PlayActivated;
            _app.Deactivated -= _view.PlayDeactivated;
        }
    }
}