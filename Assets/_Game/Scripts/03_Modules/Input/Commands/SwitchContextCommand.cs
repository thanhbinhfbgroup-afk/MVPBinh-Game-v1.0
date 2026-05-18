using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    // Command nội bộ yêu cầu chuyển input context.
    // Khi switch context, CommandBuffer phải được clear để tránh command cũ lọt sang context mới.
    public readonly struct SwitchContextCommand : ICommand
    {
        public SwitchContextCommand(
            EntityId controlledEntityId,
            float timestamp,
            InputContext nextContext)
        {
            ControlledEntityId = controlledEntityId;
            Timestamp = timestamp;
            NextContext = nextContext;
        }

        public EntityId ControlledEntityId { get; }
        public CommandType Type => CommandType.SwitchContext;
        public float Timestamp { get; }

        public InputContext NextContext { get; }
    }
}