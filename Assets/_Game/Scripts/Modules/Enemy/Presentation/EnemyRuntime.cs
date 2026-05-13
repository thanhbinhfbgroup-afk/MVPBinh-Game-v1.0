using System;
using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.Enemy.Application;
using BillGameCore.Modules.Enemy.Domain;

namespace BillGameCore.Modules.Enemy.Presentation
{
    // Handle bất biến cho một instance Enemy đang sống.
    // Trả về bởi EnemySpawner. Gọi Dispose() để dọn dẹp event subscription.
    // R04: KHÔNG BAO GIỜ đăng ký vào DI scope — được sở hữu bởi GameBootstrapper / SceneController.
    public sealed class EnemyRuntime : IDisposable
    {
        public EnemyRuntime(EntityId id, EnemyDefinition def, EnemyState state,
                          EnemyApplication app, EnemyView view, EnemyPresenter presenter)
        {
            Id          = id;
            Definition  = def;
            State       = state;
            Application = app;
            View        = view;
            Presenter   = presenter;
        }

        public EntityId        Id          { get; }
        public EnemyDefinition   Definition  { get; }
        public EnemyState        State       { get; }
        public EnemyApplication  Application { get; }
        public EnemyView         View        { get; }
        public EnemyPresenter    Presenter   { get; }

        public void Dispose() => Presenter.Dispose();
    }
}