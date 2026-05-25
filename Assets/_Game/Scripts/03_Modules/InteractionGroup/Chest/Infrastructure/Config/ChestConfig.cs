using BillGameCore.Modules.InteractionGroup.Chest.Domain;
using UnityEngine;

namespace BillGameCore.Modules.InteractionGroup.Chest.Infrastructure.Config
{
    public sealed class ChestConfig : MonoBehaviour
    {
        [SerializeField] private bool _startsOpened;

        public ChestDefinition ToDefinition()
        {
            return new ChestDefinition(_startsOpened);
        }
    }
}