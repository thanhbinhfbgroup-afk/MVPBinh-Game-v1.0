namespace BillGameCore.Modules.Player.Domain
{
    // Mutable runtime state for ONE Player instance.
    // Owned and mutated exclusively by PlayerApplication.
    // R04: NEVER registered in DI scope.
    // R10: NEVER stored in ScriptableObject.
    // R15: Pure C# — NO UnityEngine types.
    public sealed class PlayerState
    {
        public float CurrentHealth  { get; set; }
        public float CurrentStamina { get; set; }
        public float VelocityX      { get; set; }
        public float VelocityY      { get; set; }
        public bool  IsDead         { get; set; }
        // Add entity-specific runtime fields here.
    }
}