using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    public sealed class SwitchContextCommand : ICommand
    {
        public SwitchContextCommand(BillEntityId controlledEntityId, InputContext targetContext)
        {
            ControlledEntityId = controlledEntityId;
            TargetContext = targetContext;
        }

        public CommandType Type => CommandType.SwitchContext;

        public BillEntityId ControlledEntityId { get; }

        public InputContext TargetContext { get; }
    }
}
