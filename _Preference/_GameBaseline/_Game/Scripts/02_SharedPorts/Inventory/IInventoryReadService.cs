namespace BillGameCore.SharedPorts.Inventory
{
    public interface IInventoryReadService
    {
        int GetAmount(string itemId);
    }
}
