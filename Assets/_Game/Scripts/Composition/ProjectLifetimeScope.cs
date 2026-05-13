using BillGameCore.Modules.Inventory.Application;
using VContainer;
using VContainer.Unity;

public class ProjectLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<InventoryService>(Lifetime.Singleton).AsImplementedInterfaces();
    }
}
