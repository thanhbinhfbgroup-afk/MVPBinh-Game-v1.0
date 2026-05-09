// [MODULE: Player]
// [TYPE: Signal]
// [SCOPE: SceneLifetimeScope]
using UnityEngine;

namespace BillGameCore.Interfaces.Signals
{
    public struct PlayerStateChangedSignal
    {
        public bool IsMoving;
        public Vector2 FacingDirection;

        public PlayerStateChangedSignal(bool isMoving, Vector2 facingDirection)
        {
            IsMoving = isMoving;
            FacingDirection = facingDirection;
        }
    }
}