namespace BillGameCore.SharedPorts.Economy
{
    // Tiêu thụ bởi: UI/HUD. Implement bởi: EconomyService.
    public interface IWalletService
    {
        int Gold       { get; }
        int Experience { get; }
    }
}