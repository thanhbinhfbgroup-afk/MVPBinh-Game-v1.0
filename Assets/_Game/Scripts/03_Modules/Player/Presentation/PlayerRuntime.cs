using System;
using BillGameCore.Core.ValueObjects;

namespace BillGameCore.Modules.Player.Presentation
{
    public sealed class PlayerRuntime : IDisposable
    {
        public PlayerRuntime(EntityId id, PlayerView view, PlayerPresenter presenter)
        {
            if (!id.IsValid)
            {
                throw new ArgumentException("Player id must be valid.", nameof(id));
            }

            Id = id;
            View = view ?? throw new ArgumentNullException(nameof(view));
            Presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        }

        public EntityId Id { get; }

        public PlayerView View { get; }

        public PlayerPresenter Presenter { get; }

        public void Dispose()
        {
            Presenter.Dispose();
        }
    }
}