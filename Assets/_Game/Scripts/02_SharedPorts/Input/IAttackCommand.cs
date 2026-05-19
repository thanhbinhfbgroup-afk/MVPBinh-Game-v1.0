namespace BillGameCore.SharedPorts.Input
{
    public interface IAttackCommand : ICommand
    {
        bool IsHeld { get; }
        float HeldDuration { get; }
    }
}