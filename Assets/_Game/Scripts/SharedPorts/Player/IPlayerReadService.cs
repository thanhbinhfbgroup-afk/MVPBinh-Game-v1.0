namespace BillGameCore.SharedPorts.Player
{
    // Consumed by: UI/HUD. Implemented by: PlayerApplication.
    public interface IPlayerReadService
    {
        float CurrentHealth  { get; }
        float MaxHealth      { get; }
        float CurrentStamina { get; }
    }
}