using BillGameCore.Modules.InteractionGroup.Chest.Domain;

namespace BillGameCore.Modules.InteractionGroup.Chest.Application
{
    public sealed class ChestApplication
    {
        private readonly ChestState _state;

        public ChestApplication(ChestState state)
        {
            _state = state;
        }
        public bool IsOpened => _state.IsOpened;

        public bool CanInteract()
        {
            return !_state.IsOpened;
        }

        public ChestOpenResult TryOpen()
        {
            var openedNow = _state.TryOpen();
            return new ChestOpenResult(openedNow);
        }
    }
}