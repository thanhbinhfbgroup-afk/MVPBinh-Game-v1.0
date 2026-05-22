namespace BillGameCore.SharedPorts.Input
{
    public interface IInputCommandSource
    {
        bool TryDequeue(out ICommand command);
    }
}
