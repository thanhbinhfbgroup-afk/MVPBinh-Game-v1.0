using BillGameCore.Interfaces;
using BillGameCore.Interfaces.Signals;
using VContainer;
using VContainer.Unity;
using MessagePipe;
using BillGameCore.Modules.Input;
public class ProjectLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Đăng ký Signals
        var options = builder.RegisterMessagePipe();
        builder.RegisterMessageBroker<MoveInputSignal>(options);
        builder.RegisterMessageBroker<ActionInputSignal>(options);
        // Đăng ký Service
        builder.Register<IInputService, InputManager>(Lifetime.Singleton).AsImplementedInterfaces();
    }
}
