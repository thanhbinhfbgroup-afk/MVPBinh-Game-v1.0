using BillGameCore.Interfaces;
using UnityEngine;

namespace BillGameCore.Interfaces
{
    public interface IPlayerService : IService
    {
        float CurrentHp { get; }
        float CurrentStamina { get; }
        float CurrentHunger { get; }

        void TakeDamage(float amount);
        void ConsumeStamina(float amount);
        void Eat(float nutrition);
    }
}