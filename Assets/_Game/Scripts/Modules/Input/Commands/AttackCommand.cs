using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    public sealed class AttackCommand : ICommand
    {
        public AttackCommand(EntityId sourceId, float timestamp,
                             bool isHeld = false, float heldDuration = 0f)
        {
            SourceId     = sourceId;
            Timestamp    = timestamp;
            IsHeld       = isHeld;
            HeldDuration = heldDuration;
        }

        public EntityId    SourceId     { get; }
        public CommandType Type         => CommandType.Attack;
        public float       Timestamp    { get; }
        public bool        IsHeld       { get; }   // true while button held
        public float       HeldDuration { get; }   // seconds held so far
    }
}