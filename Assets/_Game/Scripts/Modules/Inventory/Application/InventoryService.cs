using System;
using BillGameCore.Core.Save;
using BillGameCore.Modules.Inventory.Domain;
using BillGameCore.Modules.Inventory.Infrastructure.Persistence;

namespace BillGameCore.Modules.Inventory.Application
{
    // System service.
    // ADR-06: đăng ký trong ProjectLifetimeScope (Save/Inventory/Economy/Audio)
    //         hoặc SceneLifetimeScope cho service chỉ sống trong một scene.
    // R07: module khác inject interface SharedPorts — không bao giờ ref class này trực tiếp.
    // R04: InventoryState được tạo ngay tại đây, không inject từ ngoài.
    public sealed class InventoryService
        : ISaveSnapshotProvider<InventorySaveData>,
          ISaveSnapshotConsumer<InventorySaveData>
    {
        private readonly InventoryState _state = new InventoryState();

        // Thông báo cho UI Presenter cục bộ — không dùng cho cross-module broadcast (dùng MessagePipe).
        public event Action Changed;

        // ── ISaveSnapshotProvider ──────────────────────────
        public InventorySaveData CreateSnapshot()
        {
            return new InventorySaveData(); // điền từ các field của _state
        }

        // ── ISaveSnapshotConsumer ──────────────────────────
        public void RestoreSnapshot(InventorySaveData snapshot)
        {
            if (snapshot == null) return;
            // Khôi phục _state từ các field trong snapshot.
            Changed?.Invoke();
        }

        // Thêm command/query method ở đây.
        // Khai báo interface hẹp trong SharedPorts/ cho module khác consume (R07).
    }
}