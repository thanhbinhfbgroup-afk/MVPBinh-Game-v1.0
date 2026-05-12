namespace BillGameCore.Modules.Player.Domain
{
    // Static gameplay data. Populated by PlayerConfig.ToDefinition(). Immutable at runtime.
    // R15: Pure C# — NO UnityEngine types in Domain.
    [System.Serializable]
    public sealed class PlayerDefinition
    {
        public float MoveSpeed = 5f;
        public float MaxHealth = 100f;
        public float MaxStamina = 100f;
        // Add entity-specific stats here.
    }
}