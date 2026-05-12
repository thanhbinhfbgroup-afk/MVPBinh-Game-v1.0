namespace BillGameCore.SharedPorts.Economy
{
    public interface IWalletService
    {
        int Gold       { get; }
        int Experience { get; }
    }
}