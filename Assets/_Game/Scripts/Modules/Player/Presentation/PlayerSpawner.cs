using BillGameCore.Core.Rewards;
using BillGameCore.Modules.Player.Application;
using BillGameCore.Modules.Player.Domain;
using BillGameCore.Modules.Player.Infrastructure.Config;
using BillGameCore.SharedPorts.Input;
using UnityEngine;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;

namespace BillGameCore.Modules.Player.Presentation
{
    // Đăng ký trong SceneLifetimeScope.
    // Nhận prefab, config và IInputCommandSource qua DI constructor injection.
    // Tạo Runtime per-entity — KHÔNG BAO GIỜ đăng ký ngược lại vào DI (R04).
    // R18: EntityId.New() được gọi tại đây — vị trí DUY NHẤT hợp lệ cho entity type này.
    // FIX-04: Không tham chiếu BillGameCore.Modules.Input — chỉ dùng SharedPorts (R06/R07).
    // FIX-08: Using BillGameCore.Modules.Player.Infrastructure.Config cho PlayerConfig.
    // FIX-11: rewardBundleFactory truyền vào PlayerApplication để tránh protected virtual (sealed).
    public sealed class PlayerSpawner
    {
        private readonly PlayerView             _prefab;
        private readonly PlayerConfig           _config;
        private readonly IInputCommandSource _inputSource;

        public PlayerSpawner(PlayerView prefab, PlayerConfig config, IInputCommandSource inputSource)
        {
            _prefab      = prefab;
            _config      = config;
            _inputSource = inputSource;
        }

        public PlayerRuntime Spawn(Vector2 position)
        {
            var id    = EntityId.New();                    // R18
            var def   = _config.ToDefinition();
            var state = new PlayerState();

            var app   = new PlayerApplication(id, def, state);

            // R17: Object.Instantiate đúng ở đây vì PlayerView không có [Inject] field.
            // Nếu thêm [Inject] vào PlayerView sau này, đổi sang container.Instantiate().
            var view      = UnityEngine.Object.Instantiate(_prefab, position, Quaternion.identity);
            var presenter = new PlayerPresenter(app, view, _inputSource);
            view.Bind(presenter);

            return new PlayerRuntime(id, def, state, app, view, presenter);
        }
    }
}
