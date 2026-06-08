namespace BillGameCore.Modules.Player.Domain
{
    public sealed class PlayerState
    {
        private float _currentHealth;

        public PlayerState(float maxHealth)
        {
            _currentHealth = maxHealth;
        }

        public float CurrentHealth => _currentHealth;

        public bool IsDead => _currentHealth <= 0f;

        public void ApplyDamage(
            float amount,
            out float appliedDamage,
            out bool justDied)
        {
            var wasDead = IsDead;

            if (amount < 0f)
            {
                amount = 0f;
            }

            if (wasDead)
            {
                appliedDamage = 0f;
                justDied = false;
                return;
            }

            var healthBefore = _currentHealth;
            _currentHealth -= amount;

            if (_currentHealth < 0f)
            {
                _currentHealth = 0f;
            }

            appliedDamage = healthBefore - _currentHealth;
            justDied = !wasDead && IsDead;
        }
    }
}