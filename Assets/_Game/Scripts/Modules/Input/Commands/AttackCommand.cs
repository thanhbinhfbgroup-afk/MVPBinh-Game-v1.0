using BillGameCore.Core.ValueObjects;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Commands
{
    // Implement cả IAttackCommand để Entity Presenter đọc IsHeld/HeldDuration (FIX-03).
    public sealed class AttackCommand : IAttackCommand
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
        public bool        IsHeld       { get; }   // true khi đang giữ nút
        public float       HeldDuration { get; }   // số giây đã giữ
    }
}