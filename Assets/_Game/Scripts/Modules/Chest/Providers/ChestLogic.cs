// [MODULE: Chest]
// [TYPE: Logic]
// [SCOPE: SceneLifetimeScope]
// [SIGNAL_PUBLISHES: ChestActivatedSignal]
// [DEPENDS_ON: none]
// Logic: xử lý trạng thái — KHÔNG biết Unity
using MessagePipe;
using VContainer;
using System;

namespace BillGameCore.Modules.Chest
{
    public class ChestLogic
    {
        public bool IsActivated { get; private set; }

        public event Action OnActivated;
        public event Action OnReset;

        private readonly IPublisher<Interfaces.Signals.ChestActivatedSignal> _publisher;

        [Inject]
        public ChestLogic(IPublisher<Interfaces.Signals.ChestActivatedSignal> publisher)
        {
            _publisher = publisher;
        }

        public void Interact(string uniqueId)
        {
            // Logic xử lý dựa trên ID duy nhất này
            UnityEngine.Debug.Log($"Đang tương tác với vật thể có ID: {uniqueId}");
        }

        public void Reset()
        {
            IsActivated = false;
            OnReset?.Invoke();
        }
    }
}