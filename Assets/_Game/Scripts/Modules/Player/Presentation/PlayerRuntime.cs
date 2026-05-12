using System;
using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.Player.Application;
using BillGameCore.Modules.Player.Domain;

namespace BillGameCore.Modules.Player.Presentation
{
    // Immutable handle for one live Player instance.
    // Returned by PlayerSpawner. Call Dispose() to clean up event subscriptions.
    public sealed class PlayerRuntime : IDisposable
    {
        public PlayerRuntime(EntityId id, PlayerDefinition def, PlayerState state,
                          PlayerApplication app, PlayerView view, PlayerPresenter presenter)
        {
            Id          = id;
            Definition  = def;
            State       = state;
            Application = app;
            View        = view;
            Presenter   = presenter;
        }

        public EntityId        Id          { get; }
        public PlayerDefinition  Definition  { get; }
        public PlayerState       State       { get; }
        public PlayerApplication Application { get; }
        public PlayerView        View        { get; }
        public PlayerPresenter   Presenter   { get; }

        public void Dispose() => Presenter.Dispose();
    }
}