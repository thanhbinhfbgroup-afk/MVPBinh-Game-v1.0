using System;
using BillGameCore.Core.Save;
using BillGameCore.Modules.Inventory.Domain;
using BillGameCore.Modules.Inventory.Infrastructure;

namespace BillGameCore.Modules.Inventory.Application
{
    // System service.
    // ADR-06: register in ProjectLifetimeScope (Save/Inventory/Economy/Audio)
    //         or SceneLifetimeScope for scene-only services.
    // R07: consuming modules inject a SharedPorts interface — never this class directly.
    // R04: InventoryState is created here, not injected from outside.
    public sealed class InventoryService
        : ISaveSnapshotProvider<InventorySaveData>,
          ISaveSnapshotConsumer<InventorySaveData>
    {
        private readonly InventoryState _state = new InventoryState();

        // Notify local UI presenters — not for cross-module broadcast (use MessagePipe for that).
        public event Action Changed;

        // ── ISaveSnapshotProvider ──────────────────────────
        public InventorySaveData CreateSnapshot()
        {
            return new InventorySaveData(); // populate from _state fields
        }

        // ── ISaveSnapshotConsumer ──────────────────────────
        public void RestoreSnapshot(InventorySaveData snapshot)
        {
            if (snapshot == null) return;
            // Restore _state from snapshot.
            Changed?.Invoke();
        }

        // Add command/query methods here.
        // Expose narrow interfaces in SharedPorts/ for other modules to consume.
    }
}