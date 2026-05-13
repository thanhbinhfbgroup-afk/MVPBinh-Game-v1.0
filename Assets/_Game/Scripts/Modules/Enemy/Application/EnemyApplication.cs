using System;
using BillGameCore.Core.Combat;
using BillGameCore.Core.Rewards;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;
using BillGameCore.Modules.Enemy.Domain;
using BillGameCore.SharedPorts.Player;
using UnityEngine;

namespace BillGameCore.Modules.Enemy.Application
{
    // Logic use-case của Enemy.
    // Implement IDamageReceiver — CombatApplication gọi ReceiveDamage() qua shared contract.
    // Implement IPlayerReadService — UI/HUD inject qua SharedPorts (không qua Modules.Enemy).
    // R15: KHÔNG có MonoBehaviour, Transform, Animator, Rigidbody2D, ScriptableObject ở đây.
    // R07: KHÔNG tham chiếu trực tiếp namespace module khác — dùng SharedPorts contracts.
    //
    // GHI CHÚ: IPlayerReadService đặc thù cho Player. Với archetype Enemy, xóa interface đó
    //          và các property tương ứng — UI không bao giờ đọc stat enemy trực tiếp.
    //
    // FIX-11: rewardBundleFactory là Func<RewardBundle> inject vào constructor.
    //         Enemy Spawner truyền: () => new RewardBundle { Gold = def.Gold, ... }
    //         Player Spawner truyền: null (sẽ dùng bundle rỗng mặc định).
    public sealed class EnemyApplication : IDamageReceiver, IPlayerReadService
    {
        private readonly EnemyDefinition        _def;
        private readonly EnemyState             _state;
        private readonly Func<RewardBundle>   _rewardBundleFactory; // FIX-11

        // FIX-02: Event mang EntityId + RewardBundle + vị trí thế giới để SceneController
        //         gọi LootSpawner.Spawn(bundle, pos) và IRewardGrantService.Grant(bundle).
        //         Với archetype Player, RewardBundle sẽ null/rỗng — điều đó là bình thường.
        public event Action<EntityId, RewardBundle, Vector2> OnDied;

        // R18: EntityId do Spawner cung cấp — không bao giờ gọi EntityId.New() ở đây.
        public EntityId Id { get; }

        // FIX-11: rewardBundleFactory có thể null (Player không drop loot).
        public EnemyApplication(EntityId id, EnemyDefinition def, EnemyState state,
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

            float applied = System.Math.Min(damage.Amount, _state.CurrentHealth);
            _state.CurrentHealth -= applied;

            bool justDied = _state.CurrentHealth <= 0f;
            if (justDied)
            {
                _state.IsDead = true;
                // FIX-11: Dùng factory được inject thay vì protected virtual method.
                //         Truyền Vector2.zero — Presenter sẽ cung cấp vị trí thực từ View.
                var bundle = _rewardBundleFactory != null
                    ? _rewardBundleFactory.Invoke()
                    : new RewardBundle();
                OnDied?.Invoke(Id, bundle, Vector2.zero);
            }

            return new DamageResult(applied, _state.CurrentHealth, justDied);
        }
    }
}