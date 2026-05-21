namespace BillGameCore.SharedPorts.Input
{
    public interface IInputCommandSource
    {
        IMoveCommand ReadMoveCommand();
    }
}
