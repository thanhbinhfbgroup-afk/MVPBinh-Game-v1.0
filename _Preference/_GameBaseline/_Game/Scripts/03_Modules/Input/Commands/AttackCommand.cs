using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    public sealed class AttackCommand : IAttackCommand
    {
        public AttackCommand(BillEntityId controlledEntityId, bool isPerformed)
        {
            ControlledEntityId = controlledEntityId;
            IsPerformed = isPerformed;
        }

        public CommandType Type => CommandType.Attack;

        public BillEntityId ControlledEntityId { get; }

        public bool IsPerformed { get; }
    }
}