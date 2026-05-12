namespace BillGameCore.SharedPorts.Input
{
    // Placed in SharedPorts so consumers don't ref Modules.Input.
    // NEVER delete or renumber existing values (replay / save compatibility).
    public enum CommandType
    {
        Move          = 0,
        Attack        = 1,
        Interact      = 2,
        SwitchContext = 3,
    }
}