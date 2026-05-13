using BillGameCore.Modules.Player.Domain;
using UnityEngine;

namespace BillGameCore.Modules.Player.Infrastructure.Config
{
    // ScriptableObject config mapper.
    // R10: CHỈ chứa config data tĩnh — không có runtime state, không có mutable field.
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "BillGameCore/Player/Player Config")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [SerializeField] private float _moveSpeed  = 5f;
        [SerializeField] private float _maxHealth  = 100f;
        [SerializeField] private float _maxStamina = 100f;
        // Thêm serialized config field khớp với PlayerDefinition.

        public PlayerDefinition ToDefinition() => new PlayerDefinition
        {
            MoveSpeed  = _moveSpeed,
            MaxHealth  = _maxHealth,
            MaxStamina = _maxStamina,
        };
    }
}