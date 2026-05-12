using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    public sealed class InteractCommand : ICommand
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