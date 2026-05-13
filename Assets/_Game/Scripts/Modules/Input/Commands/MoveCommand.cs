using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    // Implement cả IMoveCommand để Entity Presenter đọc DirX/Y mà không ref module này (FIX-03).
    public sealed class MoveCommand : IMoveCommand
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