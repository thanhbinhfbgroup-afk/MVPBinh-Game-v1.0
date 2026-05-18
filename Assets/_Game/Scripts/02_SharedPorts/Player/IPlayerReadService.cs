using BillGameCore.Core.ValueObjects;

namespace BillGameCore.SharedPorts.Player
{
    // Read-only port cho UI/HUD và hệ thống cần biết trạng thái player tối thiểu.
    // Implement bởi: PlayerApplication.
    public interface IPlayerReadService
    {
        // Dùng cho debug, command targeting và các flow cần định danh player.
        EntityId Id { get; }

        float CurrentHealth { get; }
        float MaxHealth { get; }

        float CurrentStamina { get; }
        float MaxStamina { get; }

        // Trạng thái chết do PlayerApplication quyết định, không để UI tự suy luận từ máu.
        bool IsDead { get; }
    }
}