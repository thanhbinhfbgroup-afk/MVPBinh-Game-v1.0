namespace BillGameCore.Core.Save
{
    public interface ISaveSnapshotConsumer<in TSnapshot>
    {
        void RestoreSnapshot(TSnapshot snapshot);
    }
}