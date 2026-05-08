// [MODULE: Chest]
// [TYPE: Interface]
// [SCOPE: SceneLifetimeScope]
// CHỈ giữ file này nếu module khác cần inject IChestService.
// Nếu không → xoá file này.
namespace BillGameCore.Interfaces
{
    public interface IChestService
    {
        void Interact();
        bool IsActivated { get; }
    }
}