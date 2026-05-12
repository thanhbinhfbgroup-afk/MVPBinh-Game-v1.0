using BillGameCore.Core.Rewards;

namespace BillGameCore.SharedPorts.Messages
{
    // Published by EnemyPresenter from Slice 03+ (MessagePipe unlock — ADR-03).
    // Consumed by LootSpawner and/or SceneController subscribers.
    public sealed class EnemyDiedMessage
    {
        public RewardBundle Bundle { get; }
        public float        WorldX { get; }
        public float        WorldY { get; }

        public EnemyDiedMessage(RewardBundle bundle, float worldX, float worldY)
        {
            Bundle = bundle;
            WorldX = worldX;
            WorldY = worldY;
        }
    }
}