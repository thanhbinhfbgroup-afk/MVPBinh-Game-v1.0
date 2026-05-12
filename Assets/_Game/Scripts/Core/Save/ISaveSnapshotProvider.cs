namespace BillGameCore.Core.Save
{
    public interface ISaveSnapshotProvider<out TSnapshot>
    {
        TSnapshot CreateSnapshot();
    }
}