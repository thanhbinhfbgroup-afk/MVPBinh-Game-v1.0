namespace BillGameCore.Core.Combat
{
    // Method name is ReceiveDamage — NEVER rename (CONTEXT contract).
    // Implemented by: EnemyApplication, PlayerApplication.
    // Called by: CombatApplication ONLY — never from Presenter or View.
    public interface IDamageReceiver
    {
        DamageResult ReceiveDamage(DamageInfo damage);
    }
}