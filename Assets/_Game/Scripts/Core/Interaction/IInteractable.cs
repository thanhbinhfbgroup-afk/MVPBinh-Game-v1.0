namespace BillGameCore.Core.Interaction
{
    // Implemented by Binder classes (ChestBinder, LootItemBinder ...).
    // PlayerPresenter calls GetComponent<IInteractable>() on overlap.
    // PlayerPresenter NEVER knows the concrete Binder type (R07).
    public interface IInteractable
    {
        bool CanInteract();
        void Interact();
    }
}