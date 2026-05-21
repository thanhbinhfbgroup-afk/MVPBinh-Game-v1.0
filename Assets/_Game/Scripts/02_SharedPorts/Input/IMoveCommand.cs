namespace BillGameCore.SharedPorts.Input
{
    public interface IMoveCommand: ICommand
    {
        float DirX { get; }

        float DirY { get; }

        bool IsMoving { get; }
    }
}