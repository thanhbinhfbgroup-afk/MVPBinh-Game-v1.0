using BillGameCore.Modules.Player.Domain;
using UnityEngine;

namespace BillGameCore.Modules.Player.Infrastructure.Config
{
    [CreateAssetMenu(
        fileName = "PlayerConfig",
        menuName = "BillGameCore/Player/Player Config")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [SerializeField]
        private float _moveSpeed = 5f;

        public PlayerDefinition ToDefinition()
        {
            return new PlayerDefinition(_moveSpeed);
        }
    }
}