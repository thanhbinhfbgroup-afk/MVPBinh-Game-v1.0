using BillGameCore.Core.Rewards;
using BillGameCore.Modules.Enemy.Domain;
using UnityEngine;

namespace BillGameCore.Modules.Enemy.Infrastructure.Config
{
    [CreateAssetMenu(
        fileName = "EnemyConfig",
        menuName = "BillGameCore/Enemy/Enemy Config")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _stopDistance;
        [SerializeField] private float _contactDamage;
        [SerializeField] private float _contactDamageInterval;
        [SerializeField] private int _gold;
        [SerializeField] private int _experience;
        [SerializeField] private int _coin;    

        public EnemyDefinition ToDefinition()
        {
            var reward = new RewardBundle(_gold, _experience, _coin);
            return new EnemyDefinition(_maxHealth, _moveSpeed, _stopDistance, _contactDamage, _contactDamageInterval, reward);
        }
    }
}