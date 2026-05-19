using System;
using BillGameCore.SharedPorts.Input;

namespace BillGameCore.Modules.Input.Context
{
    public static class InputContextNames
    {
        public const string Player = "Player";
        public const string UI = "UI";
        public const string Vehicle = "Vehicle";

        public static string ToActionMapName(InputContext context)
        {
            return context switch
            {
                InputContext.Player => Player,
                InputContext.UI => UI,
                InputContext.Vehicle => Vehicle,
                _ => throw new ArgumentOutOfRangeException(nameof(context), context, "Unsupported input context.")
            };
        }
    }
}