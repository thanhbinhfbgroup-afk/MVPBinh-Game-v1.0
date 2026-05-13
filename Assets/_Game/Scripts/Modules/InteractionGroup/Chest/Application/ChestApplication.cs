using System;
using BillGameCore.Modules.InteractionGroup.Chest.Domain;

namespace BillGameCore.Modules.InteractionGroup.Chest.Application
{
    // R15: Thuần C# — không có UnityEngine.
    public sealed class ChestApplication
    {
        private readonly ChestState _state;

        public event Action Activated;
        public event Action Deactivated;

        public bool IsActivated => _state.IsActivated;

        public ChestApplication(ChestDefinition def, ChestState state)
        {
            _state             = state;
            _state.IsActivated = def.StartsActivated;
        }

        public bool TryActivate()
        {
            if (_state.IsActivated) return false;
            _state.IsActivated = true;
            Activated?.Invoke();
            return true;
        }

        public void Deactivate()
        {
            if (!_state.IsActivated) return;
            _state.IsActivated = false;
            Deactivated?.Invoke();
        }
    }
}