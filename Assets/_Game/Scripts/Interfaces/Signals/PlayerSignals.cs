// [SIGNALS: Player] [SCOPE: Global — register in ProjectLifetimeScope]
// Usage: Inject IPublisher<PlayerLevelUpSignal> to publish,
//               ISubscriber<PlayerLevelUpSignal> to subscribe (via MessagePipe.VContainer)
namespace BillGameCore.Interfaces.Signals
{
    public struct PlayerLevelUpSignal
    {
        public int NewLevel;
        public PlayerLevelUpSignal(int level) => NewLevel = level;
    }

    // [ADD MORE PLAYER SIGNALS HERE]
    // public struct PlayerDiedSignal { }
    // public struct PlayerRespawnedSignal { public UnityEngine.Vector3 Position; }
}