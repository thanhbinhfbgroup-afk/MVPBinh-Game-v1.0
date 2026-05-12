using System;
using BillGameCore.Core.Combat;
using BillGameCore.Core.ValueObjects;
using BillGameCore.Modules.Player.Domain;

namespace BillGameCore.Modules.Player.Application
{
    // Player use-case logic.
    // Implements IDamageReceiver — CombatApplication calls ReceiveDamage() via shared contract.
    // R15: NO MonoBehaviour, Transform, Animator, Rigidbody2D, ScriptableObject here.
    // R07: NO direct reference to other module namespaces — use SharedPorts contracts only.
    public sealed class PlayerApplication : IDamageReceiver
    {
        private readonly PlayerDefinition _def;
        private readonly PlayerState      _state;

        // C# event — Presenter listens and notifies SceneController (Phase 1 callback / Phase 2 MessagePipe).
        public event Action<EntityId> OnDied;

        // R18: EntityId provided by Spawner — never call EntityId.New() here.
        public EntityId Id { get; }

        public PlayerApplication(EntityId id, PlayerDefinition def, PlayerState state)
        {
            Id     = id;
            _def   = def;
            _state = state;
            _state.CurrentHealth  = def.MaxHealth;
            _state.CurrentStamina = def.MaxStamina;
        }

        public float CurrentHealth  => _state.CurrentHealth;
        public float CurrentStamina => _state.CurrentStamina;
        public float VelocityX      => _state.VelocityX;
        public float VelocityY      => _state.VelocityY;
        public bool  IsDead         => _state.IsDead;

        // Called by Presenter each frame with direction values from the command queue.
        public void Tick(float dirX, float dirY, float deltaTime)
        {
            if (_state.IsDead) return;
            _state.VelocityX = dirX * _def.MoveSpeed;
            _state.VelocityY = dirY * _def.MoveSpeed;
        }

        // IDamageReceiver — called by CombatApplication ONLY (CONTEXT Section 14A).
        public DamageResult ReceiveDamage(DamageInfo damage)
        {
            if (_state.IsDead) return new DamageResult(0f, 0f, false);

            float applied = System.Math.Min(damage.Amount, _state.CurrentHealth);
            _state.CurrentHealth -= applied;

            bool justDied = _state.CurrentHealth <= 0f;
            if (justDied)
            {
                _state.IsDead = true;
                OnDied?.Invoke(Id);
            }

            return new DamageResult(applied, _state.CurrentHealth, justDied);
        }
    }
}