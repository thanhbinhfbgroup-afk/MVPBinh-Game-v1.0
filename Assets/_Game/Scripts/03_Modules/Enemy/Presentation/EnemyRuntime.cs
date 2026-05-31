using System;
using BillGameCore.Core.ValueObjects;

namespace BillGameCore.Modules.Enemy.Presentation
{
    public sealed class EnemyRuntime : IDisposable
    {
        public EnemyRuntime(BillEntityId id, EnemyPresenter presenter)
        {
            Id = id;
            Presenter = presenter;
        }

        public BillEntityId Id { get; }

        public EnemyPresenter Presenter { get; }

        public void Dispose()
        {
            Presenter.OnDiedCallback = null;
        }
    }
}