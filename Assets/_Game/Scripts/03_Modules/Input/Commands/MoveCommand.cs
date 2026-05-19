using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    public sealed class MoveCommand : IMoveCommand
    {
        public MoveCommand(EntityId controlledEntityId, float dirX, float dirY, bool isMoving)
        {
            ControlledEntityId = controlledEntityId;
            DirX = dirX;
            DirY = dirY;
            IsMoving = isMoving;
        }

        public CommandType Type => CommandType.Move;

        public EntityId ControlledEntityId { get; }

        public float DirX { get; }

        public float DirY { get; }

        public bool IsMoving { get; }
    }
}