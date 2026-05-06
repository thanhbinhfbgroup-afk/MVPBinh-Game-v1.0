using BillGameCore.Interfaces;
using UnityEngine;

namespace BillGameCore.InputSystem
{
    public interface IInputService : IService
    {
        Vector2 MoveDirection { get; }
        bool IsInputEnabled { get; }
        void ToggleInput(bool isEnabled);
    }
}