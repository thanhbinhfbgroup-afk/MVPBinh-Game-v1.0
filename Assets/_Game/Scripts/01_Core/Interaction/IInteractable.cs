namespace BillGameCore.Core.Interaction
{
    public interface IInteractable
    {
        bool CanInteract();

        void Interact();
    }
}