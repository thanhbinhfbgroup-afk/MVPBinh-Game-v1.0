// [SIGNALS: Player] [SCOPE: Global — register in ProjectLifetimeScope]
namespace BillGameCore.Interfaces.Signals
{
    public struct PlayerLevelUpSignal
    {
        public int NewLevel;
        public PlayerLevelUpSignal(int level) => NewLevel = level;
    }
    // [ADD MORE PLAYER SIGNALS HERE]
}