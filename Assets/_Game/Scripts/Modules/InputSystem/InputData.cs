using UnityEngine;
using UnityEngine.InputSystem;

namespace BillGameCore.InputSystem
{
    [CreateAssetMenu(fileName = "InputData", menuName = "BillGameCore/Input/InputConfig")]
    public class InputData : ScriptableObject
    {
        public InputActionAsset inputActions;
        public string moveActionName = "Move";
        public string attackActionName = "Attack";
        public string interactActionName = "Interact";
    }
}
