using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    public sealed class SwitchContextCommand : ICommand
    {
        public SwitchContextCommand(EntityId sourceId, InputContext targetContext, float timestamp)
        {
            SourceId      = sourceId;
            TargetContext = targetContext;
            Timestamp     = timestamp;
        }

        public EntityId    SourceId      { get; }
        public CommandType Type          => CommandType.SwitchContext;
        public float       Timestamp     { get; }
        public InputContext TargetContext { get; }
    }
}