// [MODULE: Enemy]
// [TYPE: Logic]
// [SCOPE: SceneLifetimeScope]
// [SIGNAL_PUBLISHES: EnemyDiedSignal]
// [SIGNAL_SUBSCRIBES: none]
// [DEPENDS_ON: none]
// Logic: thuần tính toán — KHÔNG using UnityEngine (ngoại lệ: Vector2, Mathf)
using MessagePipe;
using VContainer;

namespace BillGameCore.Modules.Enemy
{
    public class EnemyLogic
    {
        private readonly IPublisher<Interfaces.Signals.EnemyDiedSignal> _publisher;

        [Inject]
        public EnemyLogic(IPublisher<Interfaces.Signals.EnemyDiedSignal> publisher)
        {
            _publisher = publisher;
        }

        public void Initialize() { }

        // TODO: implement logic thuần C#
    }
}