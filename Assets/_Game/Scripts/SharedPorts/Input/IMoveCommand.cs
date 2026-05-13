namespace BillGameCore.SharedPorts.Input
{
    // FIX-03: Interface dữ liệu hẹp để Entity Presenter đọc hướng di chuyển
    //         mà không cần ref namespace BillGameCore.Modules.Input (R06/R07).
    // Implement bởi: BillGameCore.Modules.Input.Commands.MoveCommand.
    public interface IMoveCommand : ICommand
    {
        float DirX     { get; }
        float DirY     { get; }
        bool  IsMoving { get; }
    }
}