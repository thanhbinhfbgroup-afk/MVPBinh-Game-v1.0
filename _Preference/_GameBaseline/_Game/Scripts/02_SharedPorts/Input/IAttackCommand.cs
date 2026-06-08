namespace BillGameCore.SharedPorts.Input
{
    public interface IAttackCommand : ICommand
    {
        bool IsPerformed { get; }
    }
}