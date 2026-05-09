using BillGameCore.Interfaces.Signals;
using BillGameCore.Modules.Input.Interfaces;
using BillGameCore.Modules.Input.Providers;
using MessagePipe;
using VContainer;
using VContainer.Unity;
using UnityEngine;

public class SceneLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Dòng log này để chắc chắn hàm này có chạy
        Debug.Log($"[CHECK] Scope {gameObject.name} is running!");

        var options = builder.RegisterMessagePipe();


        builder.RegisterMessageBroker<MoveInputChangedSignal>(options);
        builder.RegisterMessageBroker<AttackInputSignal>(options);


        // Thử dùng Instance trực tiếp thay vì tìm trong Hierarchy để loại trừ lỗi tìm kiếm
        var input = FindObjectOfType<InputManager>();
        if (input != null)
        {

            builder.RegisterComponentInHierarchy<InputManager>().AsImplementedInterfaces();
            UnityEngine.Debug.Log("InputManager found and registered!");
        }
        else
        {
            UnityEngine.Debug.LogError("InputManager NOT FOUND in scene!");
        }

    }

}

