using BillGameCore.SharedPorts.Input;
using BillGameCore.Core.ValueObjects;

namespace BillGameCore.Modules.Input.Commands

{
   public sealed class MoveCommand : IMoveCommand

    {
        public CommandType Type => CommandType.Move;
        public EntityId ControlledEntityId { get; }
        public MoveCommand(EntityId controlledEntityId, float dirX, float dirY)
        {
            ControlledEntityId = controlledEntityId;
            DirX = dirX;
            DirY = dirY;
            IsMoving = dirX != 0f || dirY != 0f;
        }

        public float DirX { get; }

        public float DirY { get; }

        public bool IsMoving { get; }
    }
}