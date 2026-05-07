// [MODULE: Player] [MANAGER]
// [SCOPE: ProjectLifetimeScope] [DEPENDS_ON: IPublisher<PlayerLevelUpSignal>]
// [SIGNAL_PUBLISHES: PlayerLevelUpSignal]
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

        // Injected by VContainer — no hard references
        private readonly IPublisher<PlayerLevelUpSignal> _levelUpPublisher;

        [Inject]
        public PlayerManager(IPublisher<PlayerLevelUpSignal> levelUpPublisher)
        {
            _levelUpPublisher = levelUpPublisher;
        }

        public void Initialize()
        {
            Debug.Log("[PlayerManager] Initialized.");
        }

        public void Move(float speed)
        {
            Debug.Log($"[PlayerManager] Moving at speed: {speed}");
        }

        public int GetLevel() => _level;

        public void LevelUp()
        {
            _level++;
            _levelUpPublisher.Publish(new PlayerLevelUpSignal(_level));
            Debug.Log($"[PlayerManager] Level up → {_level}");
        }
    }
}