namespace BillGameCore.SharedPorts.Input
{
    public interface IMoveCommand
    {
        float DirX { get; }

        float DirY { get; }

        bool IsMoving { get; }
    }
}