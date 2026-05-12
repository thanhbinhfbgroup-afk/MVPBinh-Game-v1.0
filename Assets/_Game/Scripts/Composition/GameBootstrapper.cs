using UnityEngine;
using VContainer.Unity;

namespace BillGameCore.Composition
{
    // Scene entry point — constructor injection via VContainer.
    // Add dependencies and startup calls as slices are merged.
    public sealed class GameBootstrapper : IStartable
    {
        // Example (Slice 02):
        // private readonly PlayerSpawner _playerSpawner;
        // private readonly InputReader   _inputReader;
        //
        // public GameBootstrapper(PlayerSpawner playerSpawner, InputReader inputReader)
        // {
        //     _playerSpawner = playerSpawner;
        //     _inputReader   = inputReader;
        // }

        public void Start()
        {
            Debug.Log("[GameBootstrapper] Scene started.");

            // Example (Slice 02):
            // var runtime = _playerSpawner.Spawn(Vector2.zero);
            // _inputReader.SetControlledEntity(runtime.Id);
        }
    }
}