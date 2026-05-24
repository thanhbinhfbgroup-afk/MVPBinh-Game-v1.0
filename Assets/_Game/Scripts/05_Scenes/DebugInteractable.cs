using BillGameCore.Core.Interaction;
using UnityEngine;

namespace BillGameCore.Scenes
{
    public sealed class DebugInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _label = "DebugInteractable";

        public bool CanInteract()
        {
            return true;
        }

        public void Interact()
        {
            Debug.Log($"Interacted with '{_label}'.", this);
        }
    }
}