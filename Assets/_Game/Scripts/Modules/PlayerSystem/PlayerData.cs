using UnityEngine;

namespace BillGameCore.Player
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "BillGameCore/Player/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [Header("Movement")]
        public float moveSpeed = 5f;

        [Header("Stats")]
        public float maxHp = 100f;
        public float maxStamina = 100f;
        public float maxHunger = 100f;

        [Header("Decay/Regen")]
        public float hungerDecayRate = 0.5f; // Giảm mỗi giây
        public float staminaRegenRate = 5f;  // Hồi mỗi giây
    }
}