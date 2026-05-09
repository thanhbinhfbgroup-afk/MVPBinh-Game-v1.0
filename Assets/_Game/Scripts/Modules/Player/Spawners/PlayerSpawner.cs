// [MODULE: Player]
// [TYPE: Spawner]
// [SCOPE: SceneLifetimeScope]
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace BillGameCore.Modules.Player.Spawners
{
    public class PlayerSpawner
    {
        private readonly IObjectResolver _resolver;

        [Inject]
        public PlayerSpawner(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public void Spawn(GameObject prefab, Vector3 position)
        {        
            var go = Object.Instantiate(prefab, position, Quaternion.identity);           
            _resolver.InjectGameObject(go);
        }
    }
}