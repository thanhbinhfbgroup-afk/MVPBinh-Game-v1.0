using BillGameCore.Core.ValueObjects;

namespace BillGameCore.Core.Combat
{
    // Built by CombatApplication. Passed to IDamageReceiver.ReceiveDamage().
    // Pure C# — NO UnityEngine references allowed in Core.
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