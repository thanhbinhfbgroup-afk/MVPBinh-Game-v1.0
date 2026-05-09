// [MODULE: Input]
// [TYPE: Interface]
// [SCOPE: SceneLifetimeScope]
// [SIGNAL_PUBLISHES: none]
// [SIGNAL_SUBSCRIBES: none]
// [DEPENDS_ON: IBaseService]
// [REGISTER_IN: SceneLifetimeScope.cs -> Scene services block]
using UnityEngine;
using BillGameCore.Interfaces;

namespace BillGameCore.Modules.Input.Interfaces
{
    public interface IInputService : IBaseService
    {
        Vector2 GetMoveDirection();
        bool IsAttackPressed();
        bool IsInteractPressed();
    }
}
