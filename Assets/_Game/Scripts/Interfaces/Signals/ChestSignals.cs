// [MODULE: Chest]
// [TYPE: Signal]
// [SCOPE: SceneLifetimeScope]
// TODO: uncomment và đăng ký trong SceneLifetimeScope nếu cần:
// builder.RegisterMessageBroker<ChestActivatedSignal>(options);
namespace BillGameCore.Interfaces.Signals
{
    public struct ChestActivatedSignal
    {
        public int ObjectId;
        public ChestActivatedSignal(int id) => ObjectId = id;
    }
}