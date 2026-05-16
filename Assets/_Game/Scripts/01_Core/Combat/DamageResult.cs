namespace BillGameCore.Core.Combat
{
    // Kết quả trả về sau khi xử lý damage.
    public readonly struct DamageResult
    {
        public DamageResult(float appliedDamage, float remainingHealth, bool justDied)
        {
            AppliedDamage   = appliedDamage;
            RemainingHealth = remainingHealth;
            JustDied        = justDied;
        }

        public float AppliedDamage   { get; }
        public float RemainingHealth { get; }
        public bool  JustDied        { get; }
    }
}