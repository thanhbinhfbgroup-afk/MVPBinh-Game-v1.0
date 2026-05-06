using BillGameCore.InputSystem;
using BillGameCore.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ProjectLifetimeScope : LifetimeScope
{
    [SerializeField] private InputData inputConfig;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // 2. Đăng ký InputManager vừa là Service, vừa là EntryPoint (để chạy Initialize/Tick)
        builder.RegisterEntryPoint<InputManager>(Lifetime.Singleton)
               .As<IInputService>()
               .WithParameter(inputConfig);
    }
}
