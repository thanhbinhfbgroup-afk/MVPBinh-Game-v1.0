using UnityEngine;

namespace BillGameCore.Modules.Inventory.Infrastructure.Config
{
    // R10: Chỉ chứa config data tĩnh.
    [CreateAssetMenu(fileName = "InventorySettings", menuName = "BillGameCore/Inventory/Inventory Settings")]
    public sealed class InventorySettings : ScriptableObject
    {
        // Thêm serialized config field ở đây.
    }
}