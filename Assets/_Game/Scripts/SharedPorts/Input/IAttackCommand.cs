namespace BillGameCore.SharedPorts.Input
{
    // FIX-03: Interface dữ liệu hẹp để Entity Presenter đọc trạng thái Attack
    //         mà không cần ref namespace BillGameCore.Modules.Input (R06/R07).
    // Implement bởi: BillGameCore.Modules.Input.Commands.AttackCommand.
    public interface IAttackCommand : ICommand
    {
        bool  IsHeld       { get; }
        float HeldDuration { get; }
    }
}