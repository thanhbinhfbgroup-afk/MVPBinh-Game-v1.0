namespace BillGameCore.SharedPorts.Input
{
    // Đặt trong SharedPorts để consumer không cần ref Modules.Input.
    // KHÔNG XÓA hoặc đánh số lại các giá trị cũ (tương thích replay/save).
    public enum CommandType
    {
        None = 0,
        Move          = 1,
        Attack        = 2,
        Interact      = 3,
        SwitchContext = 4,
    }
}