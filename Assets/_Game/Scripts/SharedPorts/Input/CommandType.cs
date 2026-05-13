namespace BillGameCore.SharedPorts.Input
{
    // Đặt trong SharedPorts để consumer không cần ref Modules.Input.
    // KHÔNG XÓA hoặc đánh số lại các giá trị cũ (tương thích replay/save).
    public enum CommandType
    {
        Move          = 0,
        Attack        = 1,
        Interact      = 2,
        SwitchContext = 3,
    }
}