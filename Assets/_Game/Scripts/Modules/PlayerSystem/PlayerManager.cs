using BillGameCore.Core;
using BillGameCore.Interfaces;
using BillGameCore.InputSystem;
using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace BillGameCore.Player
{
    public class PlayerManager : IPlayerService, IInitializable, ITickable, IFixedTickable
    {
        private readonly IInputService _input;
        private readonly PlayerData _data;
        private readonly PlayerView _view; // Tham chiếu tới Body thực tế trong Scene

        public float CurrentHp { get; private set; }
        public float CurrentStamina { get; private set; }
        public float CurrentHunger { get; private set; }

        [Inject]
        public PlayerManager(IInputService input, PlayerData data, PlayerView view)
        {
            _input = input;
            _data = data;
            _view = view;
        }

        public void Initialize()
        {
            CurrentHp = _data.maxHp;
            CurrentStamina = _data.maxStamina;
            CurrentHunger = _data.maxHunger;
        }

        public void Tick()
        {
            // 1. Quản lý Hunger (Giảm dần theo thời gian)
            CurrentHunger = Mathf.Max(0, CurrentHunger - _data.hungerDecayRate * Time.deltaTime);

            // 2. Quản lý Stamina (Hồi phục tự động)
            if (CurrentStamina < _data.maxStamina)
                CurrentStamina = Mathf.Min(_data.maxStamina, CurrentStamina + _data.staminaRegenRate * Time.deltaTime);

            // 3. Báo tin cho UI
            PublishStats();
        }

        public void FixedTick()
        {
            // Xử lý di chuyển vật lý qua View
            Vector2 moveDir = _input.MoveDirection;
            _view.Move(moveDir * _data.moveSpeed);
            _view.Animate(moveDir);
            _view.Flip(moveDir.x);
        }

        public void TakeDamage(float amount)
        {
            CurrentHp = Mathf.Max(0, CurrentHp - amount);
            if (CurrentHp <= 0) EventBus.Publish(new PlayerDeadSignal());
        }

        public void ConsumeStamina(float amount) => CurrentStamina = Mathf.Max(0, CurrentStamina - amount);
        public void Eat(float nutrition) => CurrentHunger = Mathf.Min(_data.maxHunger, CurrentHunger + nutrition);

        private void PublishStats()
        {
            EventBus.Publish(new PlayerStatChangedSignal
            {
                HpPercent = CurrentHp / _data.maxHp,
                StaminaPercent = CurrentStamina / _data.maxStamina,
                HungerPercent = CurrentHunger / _data.maxHunger
            });
        }
    }
}