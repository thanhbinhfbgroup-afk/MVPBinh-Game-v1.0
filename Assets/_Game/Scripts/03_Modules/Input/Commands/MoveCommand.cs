using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    // Concrete command nội bộ Modules.Input.
    // Consumer ngoài module chỉ đọc qua IMoveCommand/ICommand.
    public readonly struct MoveCommand : IMoveCommand
    {
        public MoveCommand(
            EntityId controlledEntityId,
            float timestamp,
            float dirX,
            float dirY)
        {
            ControlledEntityId = controlledEntityId;
            Timestamp = timestamp;
            DirX = dirX;
            DirY = dirY;
        }

        public EntityId ControlledEntityId { get; }
        public CommandType Type => CommandType.Move;
        public float Timestamp { get; }

        public float DirX { get; }
        public float DirY { get; }

        public bool IsMoving => DirX != 0f || DirY != 0f;
    }
}