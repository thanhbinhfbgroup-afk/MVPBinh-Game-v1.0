namespace BillGameCore.SharedPorts.Input
{
    // Consumed by: PlayerPresenter, AI controllers.
    // Implemented by: InputCommandDispatcher (Modules.Input).
    // Registered in SceneLifetimeScope as IInputCommandSource.
    public interface IInputCommandSource
    {
        bool TryDequeue(out ICommand command);
        bool HasCommands { get; }
    }
}