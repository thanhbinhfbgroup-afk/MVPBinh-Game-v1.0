namespace BillGameCore.SharedPorts.Input
{
    // Tiêu thụ bởi: PlayerPresenter, AI controller.
    // Implement bởi: InputCommandDispatcher (Modules.Input).
    // Đăng ký trong SceneLifetimeScope với type IInputCommandSource.
    public interface IInputCommandSource
    {
        bool TryDequeue(out ICommand command);
        bool HasCommands { get; }
    }
}