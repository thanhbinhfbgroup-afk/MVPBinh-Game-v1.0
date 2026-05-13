namespace BillGameCore.Core.Save
{
    // Implement trên Service để SaveService có thể khôi phục snapshot.
    public interface ISaveSnapshotConsumer<in TSnapshot>
    {
        void RestoreSnapshot(TSnapshot snapshot);
    }
}