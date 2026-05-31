namespace BillGameCore.Modules.Enemy.Domain
{
    public sealed class EnemyState
    {
        public bool IsDead { get; private set; }

        public bool TryMarkDead()
        {
            if (IsDead)
            {
                return false;
            }

            IsDead = true;
            return true;
        }
    }
}