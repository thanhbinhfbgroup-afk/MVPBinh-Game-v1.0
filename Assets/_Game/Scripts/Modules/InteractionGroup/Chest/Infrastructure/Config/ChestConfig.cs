using BillGameCore.Modules.InteractionGroup.Chest.Domain;
using UnityEngine;

namespace BillGameCore.Modules.InteractionGroup.Chest.Infrastructure.Config
{
    // R10: Chỉ chứa config data — không có runtime state.
    [CreateAssetMenu(fileName = "ChestConfig", menuName = "BillGameCore/Interaction/Chest Config")]
    public sealed class ChestConfig : ScriptableObject
    {
        [SerializeField] private bool _startsActivated;

        public ChestDefinition ToDefinition() => new ChestDefinition
        {
            StartsActivated = _startsActivated,
        };
    }
}