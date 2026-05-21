namespace BillGameCore.SharedPorts.Input
{
    public interface ICommand
    {
        CommandType Type { get; }
    }
}