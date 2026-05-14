using BillGameCore.Modules.Enemy.Application;
using BillGameCore.Modules.Enemy.Domain;
using BillGameCore.Modules.Enemy.Infrastructure.Config;
using UnityEngine;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;

namespace BillGameCore.Modules.Enemy.Presentation
{
    // Scene-scope spawner. Creates per-enemy runtime objects and never registers them in DI.
    public sealed class EnemySpawner
    {
        private readonly EnemyView   _prefab;
        private readonly EnemyConfig _config;

        public EnemySpawner(EnemyView prefab, EnemyConfig config)
        {
            _prefab = prefab;
            _config = config;
        }

        public EnemyRuntime Spawn(Vector2 position)
        {
            var id = EntityId.New();
            var def = _config.ToDefinition();
            var state = new EnemyState();
            var app = new EnemyApplication(id, def, state);

            var view = UnityEngine.Object.Instantiate(_prefab, position, Quaternion.identity);
            var presenter = new EnemyPresenter(app, view);
            view.Bind(presenter);

            return new EnemyRuntime(id, def, state, app, view, presenter);
        }
    }
}
