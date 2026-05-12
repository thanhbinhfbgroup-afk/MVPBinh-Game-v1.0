using BillGameCore.Core.Rewards;
using BillGameCore.SharedPorts.Economy;
using UnityEngine;
using VContainer;

// Slice 03+: using BillGameCore.SharedPorts.Messages;
// Slice 03+: using MessagePipe;

namespace BillGameCore.Scenes
{
    // Mediator — wires cross-module events using direct calls (Phase 1).
    // From Slice 03+: replace direct callbacks with MessagePipe publish/subscribe.
    // R01: No Find() or FindObjectOfType() — all refs injected via [Inject] or constructor.
    public sealed class SceneController : MonoBehaviour
    {
        [Inject] private IRewardGrantService _rewardGrant;
        // [Inject] private LootSpawner _lootSpawner;  // add when Slice 04 ready

        // Called by EnemyPresenter callback (Phase 1) — or via MessagePipe subscriber (Phase 2).
        public void HandleEnemyDied(RewardBundle bundle, float worldX, float worldY)
        {
            _rewardGrant?.Grant(bundle);
            // _lootSpawner?.Spawn(bundle, new UnityEngine.Vector2(worldX, worldY));
        }
    }
}