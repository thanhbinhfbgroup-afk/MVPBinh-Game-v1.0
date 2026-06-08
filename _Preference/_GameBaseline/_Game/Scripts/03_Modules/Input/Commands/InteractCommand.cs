using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    public sealed class InteractCommand : IInteractCommand
    {
        public InteractCommand(BillEntityId controlledEntityId, bool isPerformed)
        {
            ControlledEntityId = controlledEntityId;
            IsPerformed = isPerformed;
        }

        public CommandType Type => CommandType.Interact;

        public BillEntityId ControlledEntityId { get; }

        public bool IsPerformed { get; } //Xác nhận nút phím đã được nhấn lún xuống thành công trong frame này
    }
}