namespace BillGameCore.Core.Combat
{
    // Tên method là ReceiveDamage — KHÔNG ĐƯỢC đổi tên (hợp đồng CONTEXT).
    // Implement bởi: EnemyApplication, PlayerApplication.
    // Gọi bởi: CHỈ CombatApplication — không gọi từ Presenter hay View.
    public interface IDamageReceiver
    {
        DamageResult ReceiveDamage(DamageInfo damage);
    }
}