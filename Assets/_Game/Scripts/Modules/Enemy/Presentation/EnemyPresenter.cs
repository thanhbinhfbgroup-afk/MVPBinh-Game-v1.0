using System;
using BillGameCore.Core.Rewards;
using BillGameCore.Modules.Enemy.Application;
using UnityEngine;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;

namespace BillGameCore.Modules.Enemy.Presentation
{
    // Bridge EnemyApplication -> EnemyView. Created by EnemySpawner, never by DI.
    public sealed class EnemyPresenter : IDisposable
    {
        private readonly EnemyApplication _app;
        private readonly EnemyView        _view;

        private bool _deathPlayed;

        public Action<EntityId, RewardBundle, Vector2> OnDiedCallback;

        public EnemyPresenter(EnemyApplication app, EnemyView view)
        {
            _app = app;
            _view = view;
            _app.OnDied += HandleDied;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_app.IsDead)
            {
                if (!_deathPlayed)
                {
                    _deathPlayed = true;
                    _view.PlayDeath();
                }

                return;
            }

            _app.Tick(0f, 0f, deltaTime);
            _view.UpdateMoveAnimation(0f, 0f);
        }

        public void OnFixedUpdate() => _view.SetVelocity(_app.VelocityX, _app.VelocityY);

        public void OnTriggerEnter2D(Collider2D col)
        {
            // Enemy interaction/combat collision is added by future combat/AI slices.
        }

        public void Dispose() => _app.OnDied -= HandleDied;

        private void HandleDied(EntityId id, RewardBundle bundle)
        {
            var worldPos = _view != null ? _view.WorldPosition : Vector2.zero;
            OnDiedCallback?.Invoke(id, bundle, worldPos);
        }
    }
}
