namespace BillGameCore.Modules.Enemy.Domain
{
    public sealed class EnemyState
    {
        public EnemyState(float maxHealth)
        {
            CurrentHealth = maxHealth < 0f ? 0f : maxHealth;
            IsDead = CurrentHealth <= 0f;
        }

        public float CurrentHealth { get; private set; }

        public bool IsDead { get; private set; }

        public void ApplyDamage(float amount, out float appliedDamage, out bool justDied)
        {
            if (IsDead || amount <= 0f)
            {
                appliedDamage = 0f;
                justDied = false;
                return;
            }

            var nextHealth = CurrentHealth - amount;
            if (nextHealth < 0f)
            {
                nextHealth = 0f;
            }

            appliedDamage = CurrentHealth - nextHealth;
            CurrentHealth = nextHealth;

            justDied = CurrentHealth <= 0f;
            if (justDied)
            {
                IsDead = true;
            }
        }
    }
}