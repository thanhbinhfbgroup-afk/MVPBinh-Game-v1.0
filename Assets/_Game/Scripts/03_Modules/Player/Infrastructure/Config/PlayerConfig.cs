using UnityEngine;

namespace BillGameCore.Modules.Player.Infrastructure.Config
{
    [CreateAssetMenu(
        fileName = "PlayerConfig",
        menuName = "BillGameCore/Player/Player Config")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [Header("Core")]
        [SerializeField] private float _maxHealth = 10f;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _attackDamage = 1f;

        [Header("Attack Sensor")]
        [SerializeField] private float _attackRadius = 0.2f;
        [SerializeField] private float _attackForwardOffset = 0.1f;

        [Header("Interact Sensor")]
        [SerializeField] private float _interactRadius = 0.3f;
        [SerializeField] private float _interactForwardOffset = 0.12f;

        public float MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public float AttackDamage => _attackDamage;
        public float AttackRadius => _attackRadius;
        public float AttackForwardOffset => _attackForwardOffset;
        public float InteractRadius => _interactRadius;
        public float InteractForwardOffset => _interactForwardOffset;
    }
}