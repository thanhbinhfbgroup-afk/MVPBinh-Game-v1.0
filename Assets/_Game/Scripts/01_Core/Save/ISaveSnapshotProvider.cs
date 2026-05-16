namespace BillGameCore.Core.Save
{
    // Implement trên Service để SaveService có thể xuất snapshot.
    public interface ISaveSnapshotProvider<out TSnapshot>
    {
        TSnapshot CreateSnapshot();
    }
}