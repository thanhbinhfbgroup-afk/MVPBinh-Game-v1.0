// [MODULE: Input]
// [TYPE: Signal]
// [SCOPE: ProjectLifetimeScope]
using UnityEngine;

namespace BillGameCore.Interfaces.Signals
{
    public struct MoveInputSignal
    {
        public Vector2 Direction;
        public MoveInputSignal(Vector2 direction) => Direction = direction;
    }

    public struct ActionInputSignal
    {
        public string ActionName;
        public ActionInputSignal(string actionName) => ActionName = actionName;
    }
}