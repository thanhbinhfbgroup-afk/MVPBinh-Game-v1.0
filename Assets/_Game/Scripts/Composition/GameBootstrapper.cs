using UnityEngine;
using VContainer.Unity;

namespace BillGameCore.Composition
{
    // Entry point của scene — VContainer gọi Start() sau khi build DI xong.
    // Chỉ dùng constructor injection — không có [Inject] field (pure C# class).
    // Thêm dependency và startup call khi từng slice được merge.
    public sealed class GameBootstrapper : IStartable
    {
        // ── Ví dụ Slice 02 (bỏ comment khi Player slice được merge) ──────────
        // private readonly PlayerSpawner  _playerSpawner;
        // private readonly InputReader    _inputReader;
        // private readonly SceneController _sceneController;
        //
        // public GameBootstrapper(PlayerSpawner playerSpawner,
        //                         InputReader   inputReader,
        //                         SceneController sceneController)
        // {
        //     _playerSpawner   = playerSpawner;
        //     _inputReader     = inputReader;
        //     _sceneController = sceneController;
        // }

        public void Start()
        {
            Debug.Log("[GameBootstrapper] Scene đã khởi động.");

            // ── Slice 02: spawn player và bind input ─────────────────────────
            // var runtime = _playerSpawner.Spawn(Vector2.zero);
            // _inputReader.SetControlledEntity(runtime.Id);
            // _sceneController.SetPlayerRuntime(runtime);
        }
    }
}