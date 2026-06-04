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

        public EnemySpawner(EnemyConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public EnemyRuntime Spawn(EnemyView enemyView)
        {
            if (enemyView == null)
            {
                throw new ArgumentNullException(nameof(enemyView));
            }
            var definition = _config.ToDefinition();
            var state = new EnemyState(definition.MaxHealth);
            var application = new EnemyApplication(definition, state);

            var id = BillEntityId.New();
            var presenter = new EnemyPresenter(application, enemyView);

            enemyView.Bind(presenter);
            enemyView.SetContactDamage(id, 1f);
            presenter.Initialize();

            return new EnemyRuntime(id, presenter);
        }
    }
}