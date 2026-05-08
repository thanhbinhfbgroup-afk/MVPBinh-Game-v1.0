// [MODULE: Enemy]
// [TYPE: Provider]
// [SCOPE: SceneLifetimeScope]
// [SIGNAL_PUBLISHES: none]
// [SIGNAL_SUBSCRIBES: none]
// [DEPENDS_ON: EnemyLogic]
// Provider: MonoBehaviour — bridge Unity world → Logic (input, physics data...)
using VContainer;
using UnityEngine;

namespace BillGameCore.Modules.Enemy
{
    public class EnemyProvider : MonoBehaviour
    {
        private EnemyLogic _logic;

        [Inject]
        public void Construct(EnemyLogic logic)
        {
            _logic = logic;
        }

        private void Update()
        {
            // TODO: đọc data từ Unity world, đẩy vào _logic
        }
    }
}