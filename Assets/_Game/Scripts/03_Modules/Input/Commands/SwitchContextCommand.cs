using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    public sealed class SwitchContextCommand : ICommand
    {
        public SwitchContextCommand(BillEntityId controlledEntityId, string targetContext)
        {
            ControlledEntityId = controlledEntityId;
            TargetContext = targetContext;
        }

        public CommandType Type => CommandType.SwitchContext;

        public BillEntityId ControlledEntityId { get; }

        public string TargetContext { get; }
    }
}
