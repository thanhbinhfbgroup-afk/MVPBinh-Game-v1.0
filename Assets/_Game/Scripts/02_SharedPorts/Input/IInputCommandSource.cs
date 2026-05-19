namespace BillGameCore.SharedPorts.Input
{
    public interface IInputCommandSource
    {
        bool HasCommands { get; }

        bool TryDequeue(out ICommand command);
    }
}