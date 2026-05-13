using UnityEngine;
using VContainer.Unity;

namespace BillGameCore.Composition
{
    // Project-level placeholder only.
    // Scene-specific startup lives in BillGameCore.Scenes.SceneBootstrapper
    // to avoid Composition -> Scenes asmdef cycles.
    public sealed class GameBootstrapper : IStartable
    {
        public void Start()
        {
            Debug.Log("[GameBootstrapper] Project scope ready.");
        }
    }
}
