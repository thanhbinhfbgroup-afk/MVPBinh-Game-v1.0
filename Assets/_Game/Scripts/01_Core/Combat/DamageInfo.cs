using BillGameCore.Core.ValueObjects;

namespace BillGameCore.Core.Combat
{
    // Được tạo bởi nơi phát sinh hit/damage. Truyền vào IDamageReceiver.ReceiveDamage().
    // Thuần C# — KHÔNG có UnityEngine reference trong Core.
    public readonly struct DamageInfo
    {
        public DamageInfo(float amount, EntityId sourceId, bool isCritical = false)
        {
            Amount     = amount;
            SourceId   = sourceId;
            IsCritical = isCritical;
        }

        public float    Amount     { get; }
        public EntityId SourceId   { get; }
        public bool     IsCritical { get; }
    }
}