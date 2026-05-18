using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    // Concrete command cho yêu cầu attack.
    // Không chứa damage/weapon/target vì đó không phải trách nhiệm của Input.
    public readonly struct AttackCommand : IAttackCommand
    {
        public AttackCommand(
            EntityId controlledEntityId,
            float timestamp,
            bool isHeld,
            float heldDuration)
        {
            ControlledEntityId = controlledEntityId;
            Timestamp = timestamp;
            IsHeld = isHeld;
            HeldDuration = heldDuration;
        }

        public EntityId ControlledEntityId { get; }
        public CommandType Type => CommandType.Attack;
        public float Timestamp { get; }

        public bool IsHeld { get; }
        public float HeldDuration { get; }
    }
}