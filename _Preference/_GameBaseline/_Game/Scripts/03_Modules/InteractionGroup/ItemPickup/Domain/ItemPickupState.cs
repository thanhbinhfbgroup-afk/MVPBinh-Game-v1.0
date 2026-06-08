namespace BillGameCore.Modules.InteractionGroup.ItemPickup.Domain
{
    public sealed class ItemPickupState
    {
        public bool IsCollected { get; private set; }

        public void MarkCollected()
        {
            IsCollected = true;
        }
    }
}
