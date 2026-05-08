// [MODULE: Player]
// [TYPE: Manager]
// [SCOPE: ProjectLifetimeScope]
// [SIGNAL_PUBLISHES: PlayerLevelUpSignal]
// [SIGNAL_SUBSCRIBES: none]
// [DEPENDS_ON: none]
// [REGISTER_IN: Scripts/Composition/ProjectLifetimeScope.cs → Global Services block]
using MessagePipe;
using VContainer;
using UnityEngine;
using BillGameCore.Interfaces;
using BillGameCore.Interfaces.Signals;

namespace BillGameCore.Modules.Player
{
    public class PlayerManager : IPlayerService
    {
        private int _level = 1;
        private readonly IPublisher<PlayerLevelUpSignal> _publisher;

        [Inject]
        public PlayerManager(IPublisher<PlayerLevelUpSignal> publisher)
        {
            _publisher = publisher;
        }

        public void Initialize() => Debug.Log("[PlayerManager] Initialized.");
        public void Move(float speed) => Debug.Log($"[PlayerManager] Speed: {speed}");
        public int GetLevel() => _level;

        public void LevelUp()
        {
            _level++;
            _publisher.Publish(new PlayerLevelUpSignal(_level));
            Debug.Log($"[PlayerManager] Level up → {_level}");
        }
    }
}