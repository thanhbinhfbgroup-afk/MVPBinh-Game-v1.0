// [MODULE: Player]
// [TYPE: Spawner]
// [SCOPE: SceneLifetimeScope]
// [DEPENDS_ON: IObjectResolver]
// Spawner: tạo Player prefab và inject toàn bộ dependencies vào
// Optional cho Player vì thường chỉ có 1 Player trong scene
using VContainer;
using UnityEngine;
using VContainer.Unity;

namespace BillGameCore.Modules.Player
{
    public class PlayerSpawner
    {
        private readonly IObjectResolver _resolver;

        [Inject]
        public PlayerSpawner(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public PlayerView Spawn(GameObject prefab, Vector3 position)
        {
            var go = Object.Instantiate(prefab, position, Quaternion.identity);
            _resolver.InjectGameObject(go);
            return go.GetComponent<PlayerView>();
        }
    }
}