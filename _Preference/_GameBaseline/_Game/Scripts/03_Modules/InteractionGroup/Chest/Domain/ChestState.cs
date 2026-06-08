namespace BillGameCore.Modules.InteractionGroup.Chest.Domain
{
    public sealed class ChestState
    {
        private bool _isOpened;

        public bool IsOpened => _isOpened;

        public ChestState(ChestDefinition definition)
        {
            _isOpened = definition.StartsOpened;
        }

        public void MarkOpened()
        {
            _isOpened = true;
        }
    }
}