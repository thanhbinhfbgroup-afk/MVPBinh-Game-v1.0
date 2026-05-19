using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    public readonly struct MoveCommand : IMoveCommand
    {
        public MoveCommand(EntityId controlledEntityId, float dirX, float dirY)
        {
            ControlledEntityId = controlledEntityId;
            DirX = dirX;
            DirY = dirY;
        }

        public CommandType Type => CommandType.Move;

        public EntityId ControlledEntityId { get; }

        public float DirX { get; }

        public float DirY { get; }

        public bool IsMoving => DirX != 0f || DirY != 0f;
    }
}