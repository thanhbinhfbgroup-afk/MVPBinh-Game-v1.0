using UnityEngine;
using VContainer;
using VContainer.Unity;
using BillGameCore.Modules.Player.Spawners;

namespace BillGameCore.Modules.Player
{
    public class PlayerSpawnEntryPoint : IStartable
    {
        private readonly PlayerSpawner _spawner;
        private readonly GameObject _playerPrefab;
        private readonly Vector3 _spawnPosition;

        [Inject]
        public PlayerSpawnEntryPoint(
            PlayerSpawner spawner,
            GameObject playerPrefab,
            Vector3 spawnPosition)
        {
            _spawner = spawner;
            _playerPrefab = playerPrefab;
            _spawnPosition = spawnPosition;
        }

        public void Start()
        {
            Debug.Log($"[PlayerSpawnEntryPoint] Spawn position from DI/config: {_spawnPosition}");
            _spawner.Spawn(_playerPrefab, _spawnPosition);
        }
    }
}