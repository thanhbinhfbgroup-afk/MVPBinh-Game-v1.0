using BillGameCore.Core.Rewards;
using BillGameCore.SharedPorts.Economy;
using UnityEngine;
using EntityId = BillGameCore.Core.ValueObjects.EntityId;

// Slice 03+: using BillGameCore.SharedPorts.Messages;
// Slice 03+: using MessagePipe;

namespace BillGameCore.Scenes
{
    // Mediator — wire cross-module event bằng direct callback (Phase 1).
    // Từ Slice 03+: thay direct callback bằng MessagePipe publish/subscribe.
    // R01: Không dùng Find() hay FindObjectOfType() — mọi ref qua [Inject] hoặc constructor.
    // R06: SceneController chỉ đụng interface SharedPorts, không đụng nội bộ module.
    public sealed class SceneController : MonoBehaviour
    {
        private IRewardGrantService _rewardGrant;
        // Slice 04: [Inject] private LootSpawner _lootSpawner;

        public void SetRewardGrantService(IRewardGrantService rewardGrant)
        {
            _rewardGrant = rewardGrant;
        }

        // Được gọi bởi EnemyPresenter.OnDiedCallback sau khi EnemySpawner.Spawn() được wire bởi scene code.
        public void HandleEnemyDied(EntityId entityId, RewardBundle bundle, Vector2 worldPosition)
        {
            _rewardGrant?.Grant(bundle);
            // Slice 04: _lootSpawner?.Spawn(bundle, worldPosition);
        }

        public void HandlePlayerDied(EntityId entityId, RewardBundle bundle, Vector2 worldPosition)
        {
            Debug.Log($"[SceneController] Player died: {entityId} at {worldPosition}");
        }
    }
}
