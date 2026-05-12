using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    public sealed class MoveCommand : ICommand
    {
        public MoveCommand(EntityId sourceId, float dirX, float dirY, float timestamp)
        {
            SourceId  = sourceId;
            DirX      = dirX;
            DirY      = dirY;
            Timestamp = timestamp;
        }

        public EntityId    SourceId  { get; }
        public CommandType Type      => CommandType.Move;
        public float       Timestamp { get; }
        public float       DirX      { get; }
        public float       DirY      { get; }
        public bool        IsMoving  => DirX != 0f || DirY != 0f;
    }
}