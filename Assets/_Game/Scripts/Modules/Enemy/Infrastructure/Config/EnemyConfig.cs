using BillGameCore.Modules.Enemy.Domain;
using UnityEngine;

namespace BillGameCore.Modules.Enemy.Infrastructure.Config
{
    // ScriptableObject config mapper.
    // R10: CHỈ chứa config data tĩnh — không có runtime state, không có mutable field.
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "BillGameCore/Enemy/Enemy Config")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [SerializeField] private float _moveSpeed  = 5f;
        [SerializeField] private float _maxHealth  = 100f;
        [SerializeField] private float _maxStamina = 100f;
        // Thêm serialized config field khớp với EnemyDefinition.

        public EnemyDefinition ToDefinition() => new EnemyDefinition
        {
            MoveSpeed  = _moveSpeed,
            MaxHealth  = _maxHealth,
            MaxStamina = _maxStamina,
        };
    }
}