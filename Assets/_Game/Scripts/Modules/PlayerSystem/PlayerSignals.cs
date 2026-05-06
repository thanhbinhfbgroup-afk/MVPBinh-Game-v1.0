namespace BillGameCore.Player
{
    public struct PlayerDeadSignal { }

    public struct PlayerStatChangedSignal
    {
        public float HpPercent;
        public float StaminaPercent;
        public float HungerPercent;
    }
}