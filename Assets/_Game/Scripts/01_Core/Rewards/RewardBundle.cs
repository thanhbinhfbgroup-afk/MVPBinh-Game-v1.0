namespace BillGameCore.Core.Rewards
{
    public readonly struct RewardBundle
    {
        public RewardBundle(int gold, int experience)
        {
            Gold = gold;
            Experience = experience;
        }

        public int Gold { get; }
        public int Experience { get; }

        public bool IsEmpty => Gold <= 0 && Experience <= 0;
    }
}