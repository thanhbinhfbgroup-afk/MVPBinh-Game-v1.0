namespace BillGameCore.Modules.InteractionGroup.Chest.Application
{
    public readonly struct ChestOpenResult
    {
        public ChestOpenResult(bool isOpenedNow)
        {
            IsOpenedNow = isOpenedNow;
        }

        public bool IsOpenedNow { get; }
    }
}