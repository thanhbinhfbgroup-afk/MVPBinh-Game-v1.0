// [MODULE: Player] [INTERFACE]
// [REGISTER_IN: ProjectLifetimeScope → builder.Register<IPlayerService, PlayerManager>]
namespace BillGameCore.Interfaces
{
    public interface IPlayerService : IBaseService
    {
        void Move(float speed);
        int GetLevel();
    }
}