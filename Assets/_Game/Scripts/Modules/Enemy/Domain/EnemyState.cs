namespace BillGameCore.Modules.Enemy.Domain
{
    // Trạng thái runtime có thể thay đổi cho MỘT instance Enemy.
    // Chỉ được sở hữu và thay đổi bởi EnemyApplication.
    // R04: KHÔNG BAO GIỜ đăng ký vào DI scope.
    // R10: KHÔNG BAO GIỜ lưu trong ScriptableObject.
    // R15: Thuần C# — KHÔNG có UnityEngine types.
    public sealed class EnemyState
    {
        public float CurrentHealth  { get; set; }
        public float CurrentStamina { get; set; }
        public float VelocityX      { get; set; }
        public float VelocityY      { get; set; }
        public bool  IsDead         { get; set; }
        // Thêm trường runtime đặc thù của entity ở đây.
    }
}