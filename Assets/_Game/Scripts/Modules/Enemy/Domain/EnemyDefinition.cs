namespace BillGameCore.Modules.Enemy.Domain
{
    // Dữ liệu gameplay tĩnh. Được điền bởi EnemyConfig.ToDefinition(). Bất biến khi runtime.
    // R15: Thuần C# — KHÔNG có UnityEngine types trong Domain.
    [System.Serializable]
    public sealed class EnemyDefinition
    {
        public float MoveSpeed  = 5f;
        public float MaxHealth  = 100f;
        public float MaxStamina = 100f;
        public int   GoldReward = 0;
        public int   ExperienceReward = 0;
    }
}
