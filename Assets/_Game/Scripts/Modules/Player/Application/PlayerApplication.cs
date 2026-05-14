using System;
using BillGameCore.Core.Combat;
using BillGameCore.Core.Rewards;
using BillGameCore.Modules.Player.Domain;
using BillGameCore.SharedPorts.Player;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;

namespace BillGameCore.Modules.Player.Application
{
    // Logic use-case của Player.
    // Implement IDamageReceiver — CombatApplication gọi ReceiveDamage() qua shared contract.
    // Implement IPlayerReadService — UI/HUD inject qua SharedPorts (không qua Modules.Player).
    // R15: KHÔNG có MonoBehaviour, Transform, Animator, Rigidbody2D, ScriptableObject ở đây.
    // R07: KHÔNG tham chiếu trực tiếp namespace module khác — dùng SharedPorts contracts.
    //
    // GHI CHÚ: IPlayerReadService đặc thù cho Player. Với archetype Enemy, xóa interface đó
    //          và các property tương ứng — UI không bao giờ đọc stat enemy trực tiếp.
    //
    // FIX-11: rewardBundleFactory là Func<RewardBundle> inject vào constructor.
    //         Enemy Spawner truyền: () => new RewardBundle { Gold = def.Gold, ... }
    //         Player Spawner truyền: null (sẽ dùng bundle rỗng mặc định).
    public sealed class PlayerApplication : IDamageReceiver, IPlayerReadService
    {
        private readonly PlayerDefinition        _def;
        private readonly PlayerState             _state;
        private readonly Func<RewardBundle>   _rewardBundleFactory; // FIX-11

        // Application chỉ emit domain data. Vị trí thế giới thuộc Presentation/Scene layer.
        public event Action<EntityId, RewardBundle> OnDied;

        // R18: EntityId do Spawner cung cấp — không bao giờ gọi EntityId.New() ở đây.
        public EntityId Id { get; }

        // FIX-11: rewardBundleFactory có thể null (Player không drop loot).
        public PlayerApplication(EntityId id, PlayerDefinition def, PlayerState state,
                              Func<RewardBundle> rewardBundleFactory = null)
        {
            Id                   = id;
            _def                 = def;
            _state               = state;
            _rewardBundleFactory = rewardBundleFactory;
            _state.CurrentHealth  = def.MaxHealth;
            _state.CurrentStamina = def.MaxStamina;
        }

        // ── IPlayerReadService ────────────────────────────────────────────────
        // FIX-06/07: MaxHealth từ _def (bất biến), CurrentHealth/Stamina từ _state.
        public float CurrentHealth  => _state.CurrentHealth;
        public float MaxHealth      => _def.MaxHealth;
        public float CurrentStamina => _state.CurrentStamina;

        // Accessor nội bộ cho Presenter (write velocity ra View).
        public float VelocityX => _state.VelocityX;
        public float VelocityY => _state.VelocityY;
        public bool  IsDead    => _state.IsDead;

        // Gọi bởi Presenter mỗi frame với giá trị direction từ command queue.
        public void Tick(float dirX, float dirY, float deltaTime)
        {
            if (_state.IsDead) return;
            _state.VelocityX = dirX * _def.MoveSpeed;
            _state.VelocityY = dirY * _def.MoveSpeed;
        }

        // IDamageReceiver — CHỈ được gọi bởi CombatApplication (CONTEXT §14A).
        public DamageResult ReceiveDamage(DamageInfo damage)
        {
            if (_state.IsDead) return new DamageResult(0f, 0f, false);

            float requested = System.Math.Max(0f, damage.Amount);
            float applied = System.Math.Min(requested, _state.CurrentHealth);
            _state.CurrentHealth -= applied;

            bool justDied = _state.CurrentHealth <= 0f;
            if (justDied)
            {
                _state.IsDead = true;
                var bundle = _rewardBundleFactory != null
                    ? _rewardBundleFactory.Invoke()
                    : new RewardBundle();
                OnDied?.Invoke(Id, bundle);
            }

            return new DamageResult(applied, _state.CurrentHealth, justDied);
        }
    }
}
