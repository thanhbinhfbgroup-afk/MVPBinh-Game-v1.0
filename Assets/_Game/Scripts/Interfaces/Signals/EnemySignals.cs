// [MODULE: Enemy]
// [TYPE: Signal]
// [SCOPE: SceneLifetimeScope]
// Thêm vào SceneLifetimeScope: builder.RegisterMessageBroker<EnemyDiedSignal>(options);
namespace BillGameCore.Interfaces.Signals
{
    public struct EnemyDiedSignal
    {
        public int EntityId;
        public EnemyDiedSignal(int id) => EntityId = id;
    }
    // [ADD MORE ENEMY SIGNALS HERE]
}