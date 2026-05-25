using System;
using BillGameCore.Modules.InteractionGroup.Chest.Application;

namespace BillGameCore.Modules.InteractionGroup.Chest.Presentation
{
    public sealed class ChestPresenter
    {
        private readonly ChestApplication _application;
        private readonly ChestView _view;

        public ChestPresenter(ChestApplication application, ChestView view)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public bool CanInteract()
        {
            return _application.CanInteract();
        }

        public void Initialize()
        {
            _view.SetOpened(_application.IsOpened);
        }

        public bool TryInteract()
        {
            var result = _application.TryOpen();
            if (!result.IsOpenedNow)
            {
                return false;
            }

            _view.SetOpened(_application.IsOpened);
            return true;
        }
    }
}