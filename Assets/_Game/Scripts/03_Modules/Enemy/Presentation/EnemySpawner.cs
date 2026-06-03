using System;
using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.Enemy.Application;
using BillGameCore.Modules.Enemy.Domain;
using BillGameCore.Modules.Enemy.Infrastructure.Config;
using UnityEngine;

namespace BillGameCore.Modules.Enemy.Presentation
{
    public sealed class EnemySpawner
    {
        private readonly EnemyConfig _config;
        private readonly EnemyView _enemyView;

        public EnemySpawner(EnemyConfig config, EnemyView enemyView)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _enemyView = enemyView ?? throw new ArgumentNullException(nameof(enemyView));
        }

        public EnemyRuntime Spawn()
        {
            var definition = _config.ToDefinition();
            var state = new EnemyState(definition.MaxHealth);
            var application = new EnemyApplication(definition, state);

            var id = BillEntityId.New();
            var presenter = new EnemyPresenter(application, _enemyView);

            _enemyView.Bind(presenter);
            _enemyView.SetContactDamage(id, 1f);
            presenter.Initialize();

            return new EnemyRuntime(id, presenter);
        }
    }
}