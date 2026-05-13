namespace BillGameCore.SharedPorts.Input
{
    // Context input hiện tại — điều khiển ActionMap nào đang active.
    public enum InputContext
    {
        None    = 0,
        Player  = 1,
        Vehicle = 2,
        UI      = 3,
    }
}