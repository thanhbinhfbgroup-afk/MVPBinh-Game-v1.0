namespace BillGameCore.Core.Rewards
{
    public readonly struct RewardBundle
    {
        public RewardBundle(int gold, int experience, int coin)
        {
            Gold = gold;
            Experience = experience;
            Coin = coin;
        }

        public int Gold { get; }
        public int Experience { get; }
        public int Coin { get; }

        public bool IsEmpty => Gold <= 0 && Experience <= 0 && Coin<= 0;
    }
}