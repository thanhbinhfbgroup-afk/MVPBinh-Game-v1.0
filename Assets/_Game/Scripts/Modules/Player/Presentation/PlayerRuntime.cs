using System;
using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.Player.Application;
using BillGameCore.Modules.Player.Domain;

namespace BillGameCore.Modules.Player.Presentation
{
    // Handle bất biến cho một instance Player đang sống.
    // Trả về bởi PlayerSpawner. Gọi Dispose() để dọn dẹp event subscription.
    // R04: KHÔNG BAO GIỜ đăng ký vào DI scope — được sở hữu bởi GameBootstrapper / SceneController.
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
        public PlayerDefinition   Definition  { get; }
        public PlayerState        State       { get; }
        public PlayerApplication  Application { get; }
        public PlayerView         View        { get; }
        public PlayerPresenter    Presenter   { get; }

        public void Dispose() => Presenter.Dispose();
    }
}