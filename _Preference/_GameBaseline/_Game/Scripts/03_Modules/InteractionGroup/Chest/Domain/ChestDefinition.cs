using BillGameCore.Core.Rewards;

namespace BillGameCore.Modules.InteractionGroup.Chest.Domain
{
    public sealed class ChestDefinition
    {
        public ChestDefinition(bool startsOpened, RewardBundle reward)
        {
            StartsOpened = startsOpened;
            Reward = reward;
        }

        public bool StartsOpened { get; }
        public RewardBundle Reward { get; }
    }
}