using BillGameCore.Interfaces;
using BillGameCore.Interfaces.Signals;
using BillGameCore.Modules.Input.Interfaces;
using BillGameCore.Modules.Input.Providers;
using BillGameCore.Modules.Save;
using DG.Tweening.Core.Easing;
using MessagePipe;
using VContainer;
using VContainer.Unity;

public class ProjectLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Trong method Configure(IContainerBuilder builder)
        var options = builder.RegisterMessagePipe();
        builder.RegisterMessageBroker<GameSavedSignal>(options);
        builder.RegisterMessageBroker<GameLoadedSignal>(options);

        builder.RegisterEntryPoint<SaveManager>(Lifetime.Singleton).AsSelf().As<ISaveService>();

    }
}
