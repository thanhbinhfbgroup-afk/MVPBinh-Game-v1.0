using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    // Implement cả IInteractCommand (marker interface trong SharedPorts — FIX-03/09).
    public sealed class InteractCommand : IInteractCommand
    {
        public InteractCommand(EntityId sourceId, float timestamp)
        {
            SourceId  = sourceId;
            Timestamp = timestamp;
        }

        public EntityId    SourceId  { get; }
        public CommandType Type      => CommandType.Interact;
        public float       Timestamp { get; }
    }
}