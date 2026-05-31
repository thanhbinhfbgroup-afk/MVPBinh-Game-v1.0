using BillGameCore.Core.Rewards;
using BillGameCore.Modules.InteractionGroup.Chest.Domain;

namespace BillGameCore.Modules.InteractionGroup.Chest.Application
{
    public sealed class ChestApplication
    {
        private readonly ChestState _state;
        private readonly ChestDefinition _definition;

        public ChestApplication(ChestDefinition definition, ChestState state)
        {
            _state = state;
            _definition = definition;
        }
        public bool IsOpened => _state.IsOpened;

        public bool CanInteract()
        {
            return !_state.IsOpened;
        }

        public ChestOpenResult TryOpen()
        {
            if (_state.IsOpened)
            {
                return new ChestOpenResult(false, new RewardBundle(0, 0, 0));
            }

            _state.MarkOpened();
            return new ChestOpenResult(true, _definition.Reward);
        }

    }
}