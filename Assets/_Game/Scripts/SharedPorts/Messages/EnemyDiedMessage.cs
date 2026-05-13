using BillGameCore.Core.Rewards;
using UnityEngine;

namespace BillGameCore.SharedPorts.Messages
{
    // Publish bởi EnemyPresenter từ Slice 03+ (MessagePipe unlock — ADR-03).
    // Tiêu thụ bởi LootSpawner và/hoặc SceneController subscriber.
    public sealed class EnemyDiedMessage
    {
        public RewardBundle Bundle   { get; }
        public Vector2      Position { get; }

        public EnemyDiedMessage(RewardBundle bundle, Vector2 position)
        {
            Bundle   = bundle;
            Position = position;
        }
    }
}