namespace BillGameCore.Core.Interaction
{
    // Implement bởi các class Binder (ChestBinder, LootItemBinder ...).
    // PlayerPresenter gọi GetComponent<IInteractable>() khi overlap.
    // PlayerPresenter KHÔNG BAO GIỜ biết kiểu Binder cụ thể (R07).
    public interface IInteractable
    {
        bool CanInteract();
        void Interact();
    }
}