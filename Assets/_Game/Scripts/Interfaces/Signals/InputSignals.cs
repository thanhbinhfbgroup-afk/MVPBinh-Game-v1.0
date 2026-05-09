// [MODULE: Input]
// [TYPE: Signal]
// [SCOPE: SceneLifetimeScope]
// [SIGNAL_PUBLISHES: none]
// [SIGNAL_SUBSCRIBES: none]
// [DEPENDS_ON: none]
// [REGISTER_IN: SceneLifetimeScope.cs -> Scene signals block]
using UnityEngine;

namespace BillGameCore.Interfaces.Signals
{
    public readonly struct MoveInputChangedSignal
    {
        public readonly Vector2 Direction;

        public MoveInputChangedSignal(Vector2 direction)
        {
            Direction = direction;
        }
    }

    public readonly struct AttackInputSignal
    {
    }
}
