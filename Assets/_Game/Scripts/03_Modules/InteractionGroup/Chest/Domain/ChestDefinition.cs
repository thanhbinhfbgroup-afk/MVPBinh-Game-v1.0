namespace BillGameCore.Modules.InteractionGroup.Chest.Domain
{
    public sealed class ChestDefinition
    {
        public ChestDefinition(bool startsOpened)
        {
            StartsOpened = startsOpened;
        }

        public bool StartsOpened { get; }
    }
}