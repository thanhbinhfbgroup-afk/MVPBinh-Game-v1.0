using BillGameCore.Modules.Player.Domain;
using UnityEngine;

namespace BillGameCore.Modules.Player.Infrastructure
{
    // ScriptableObject config mapper.
    // R10: Static config data ONLY — no runtime state, no mutable fields.
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "BillGameCore/Player/Player Config")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [SerializeField] private float _moveSpeed  = 5f;
        [SerializeField] private float _maxHealth  = 100f;
        [SerializeField] private float _maxStamina = 100f;

        public PlayerDefinition ToDefinition() => new PlayerDefinition
        {
            MoveSpeed  = _moveSpeed,
            MaxHealth  = _maxHealth,
            MaxStamina = _maxStamina,
        };
    }
}