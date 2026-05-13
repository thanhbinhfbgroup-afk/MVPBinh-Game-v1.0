namespace BillGameCore.Modules.Inventory.Domain
{
    // Trạng thái runtime được sở hữu độc quyền bởi InventoryService.
    // R04: Không vào DI scope.  R10: Không trong ScriptableObject.  R15: Thuần C#.
    public sealed class InventoryState
    {
        public System.Collections.Generic.List<BillGameCore.Core.Inventory.ItemStack> Items { get; }
            = new System.Collections.Generic.List<BillGameCore.Core.Inventory.ItemStack>();
    }
}
