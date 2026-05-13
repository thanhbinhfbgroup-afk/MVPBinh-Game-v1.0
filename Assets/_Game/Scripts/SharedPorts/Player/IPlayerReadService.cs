namespace BillGameCore.SharedPorts.Player
{
    // Tiêu thụ bởi: module UI/HUD.
    // Implement bởi: PlayerApplication (Modules.Player).
    // Đăng ký sau khi spawn: builder.RegisterInstance(runtime.Application).As<IPlayerReadService>().
    public interface IPlayerReadService
    {
        float CurrentHealth  { get; }
        float MaxHealth      { get; }   // FIX-07: lấy từ Definition (config bất biến)
        float CurrentStamina { get; }
    }
}