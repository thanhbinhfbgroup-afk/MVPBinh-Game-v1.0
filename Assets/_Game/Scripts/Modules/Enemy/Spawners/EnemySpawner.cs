// [MODULE: Enemy]
// [TYPE: Spawner]
// [SCOPE: SceneLifetimeScope]
// [DEPENDS_ON: IObjectResolver]
// Spawner: Instantiate prefab, inject dependencies qua VContainer — class riêng, KHÔNG nhúng vào Scope
using VContainer;
using UnityEngine;
using VContainer.Unity;

namespace BillGameCore.Modules.Enemy
{
    public class EnemySpawner
    {
        private readonly IObjectResolver _resolver;

        [Inject]
        public EnemySpawner(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public EnemyView Spawn(GameObject prefab, Vector3 position)
        {
            var go = Object.Instantiate(prefab, position, Quaternion.identity);
            _resolver.InjectGameObject(go);
            return go.GetComponent<EnemyView>();
        }
    }
}