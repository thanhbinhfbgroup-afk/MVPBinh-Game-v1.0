namespace BillGameCore.SharedPorts.Input
{
    public interface IInteractCommand : ICommand
    {
        bool IsPerformed { get; }
    }
}