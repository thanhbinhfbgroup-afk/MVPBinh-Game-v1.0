using BillGameCore.Core.Rewards;
using BillGameCore.Modules.Enemy.Application;
using BillGameCore.Modules.Enemy.Domain;
using BillGameCore.Modules.Enemy.Infrastructure.Config;
using BillGameCore.SharedPorts.Input;
using System;
using UnityEngine;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;

namespace BillGameCore.Modules.Enemy.Presentation
{
    // Đăng ký trong SceneLifetimeScope.
    // Nhận prefab, config và IInputCommandSource qua DI constructor injection.
    // Tạo Runtime per-entity — KHÔNG BAO GIỜ đăng ký ngược lại vào DI (R04).
    // R18: EntityId.New() được gọi tại đây — vị trí DUY NHẤT hợp lệ cho entity type này.
    // FIX-04: Không tham chiếu BillGameCore.Modules.Input — chỉ dùng SharedPorts (R06/R07).
    // FIX-08: Using BillGameCore.Modules.Enemy.Infrastructure.Config cho EnemyConfig.
    // FIX-11: rewardBundleFactory truyền vào EnemyApplication để tránh protected virtual (sealed).
    public sealed class EnemySpawner
    {
        private readonly EnemyView             _prefab;
        private readonly EnemyConfig           _config;
        private readonly IInputCommandSource _inputSource;

        // FIX-11: Factory tạo RewardBundle — truyền null nếu entity không drop loot (e.g. Player).
        //         Enemy Spawner ghi đè factory này với lambda lấy loot từ EnemyDefinition.
        private readonly Func<RewardBundle> _rewardBundleFactory;

        // Constructor cho Player hoặc entity không drop loot.
        public EnemySpawner(EnemyView prefab, EnemyConfig config, IInputCommandSource inputSource)
            : this(prefab, config, inputSource, null) { }

        // Constructor cho Enemy hoặc entity có drop loot.
        public EnemySpawner(EnemyView prefab, EnemyConfig config,
                          IInputCommandSource inputSource, Func<RewardBundle> rewardBundleFactory)
        {
            _prefab              = prefab;
            _config              = config;
            _inputSource         = inputSource;
            _rewardBundleFactory = rewardBundleFactory;
        }

        public EnemyRuntime Spawn(Vector2 position)
        {
            var id    = EntityId.New();                    // R18
            var def   = _config.ToDefinition();
            var state = new EnemyState();

            // FIX-11: Truyền factory vào Application — không dùng protected virtual.
            var app   = new EnemyApplication(id, def, state, _rewardBundleFactory);

            // R17: Object.Instantiate đúng ở đây vì EnemyView không có [Inject] field.
            // Nếu thêm [Inject] vào EnemyView sau này, đổi sang container.Instantiate().
            var view      =  UnityEngine.Object.Instantiate(_prefab, position, Quaternion.identity);
            var presenter = new EnemyPresenter(app, view, _inputSource);
            view.Bind(presenter);

            return new EnemyRuntime(id, def, state, app, view, presenter);
        }
    }
}