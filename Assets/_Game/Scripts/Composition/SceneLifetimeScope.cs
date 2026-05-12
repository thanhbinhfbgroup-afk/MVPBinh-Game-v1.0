using BillGameCore.Modules.Input.Infrastructure; // Chứa InputReader và CommandBuffer
using UnityEngine.InputSystem;                   // Chứa PlayerInput
using VContainer;
using VContainer.Unity;
using BillGameCore.Modules.Input.Commands;
using BillGameCore.Modules.Input.Application;
using BillGameCore.SharedPorts.Input;

public class SceneLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 1. Đăng ký CommandBuffer (Dùng cho R16)
        builder.Register<CommandBuffer>(Lifetime.Singleton).WithParameter(32);

        // 2. Đăng ký PlayerInput (Tự động tìm nó trên GameObject hoặc Scene)
        // Cách này giúp InputReader tự có PlayerInput mà không cần kéo tay
        builder.RegisterComponentInHierarchy<PlayerInput>();

        // 3. Đăng ký InputReader (Theo Rule R17 trong ảnh của bạn)
        // RegisterComponent sẽ tự động thực hiện [Inject] cho các biến bên trong nó
        builder.RegisterComponentInHierarchy<InputReader>();

        // 4. Đăng ký Dispatcher
        builder.Register<InputCommandDispatcher>(Lifetime.Singleton).As<IInputCommandSource>();
    }
}
