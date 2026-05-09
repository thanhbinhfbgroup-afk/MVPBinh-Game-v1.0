// [MODULE: Player]
// [TYPE: Interface]
// [SCOPE: SceneLifetimeScope]
// [REGISTER_IN: Scripts/Composition/SceneLifetimeScope.cs]
using UnityEngine;

namespace BillGameCore.Interfaces
{
    public interface IMovementService : IBaseService
    {
        Vector2 CurrentVelocity { get; }
        float SneakRadiusMultiplier { get; }
    }
}