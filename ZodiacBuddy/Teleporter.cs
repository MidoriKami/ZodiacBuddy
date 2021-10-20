using Dalamud.Logging;

namespace ZodiacBuddy
{
    /// <summary>
    /// Teleporter implementation from Otter.
    /// </summary>
    internal static unsafe class Teleporter
    {
        /// <summary>
        /// Teleport to the given aetheryte.
        /// </summary>
        /// <param name="aetheryteID">Aetheryte ID.</param>
        /// <returns>If the teleport was successful.</returns>
        public static bool Teleport(uint aetheryteID)
        {
            if (Service.ClientState.LocalPlayer == null)
                return false;

            var teleport = FFXIVClientStructs.FFXIV.Client.Game.UI.Telepo.Instance();
            if (teleport == null)
            {
                Service.ChatGui.PrintError("Something horrible happened, please contact the developer.");
                PluginLog.Error("Could not teleport: Telepo is missing.");
                return false;
            }

            if (teleport->TeleportList.Size() == 0)
                teleport->UpdateAetheryteList();

            var endPtr = teleport->TeleportList.Last;
            for (var it = teleport->TeleportList.First; it != endPtr; it++)
            {
                if (it->AetheryteId == aetheryteID)
                    return teleport->Teleport(aetheryteID, 0);
            }

            Service.ChatGui.PrintError("Could not teleport, not attuned.");
            return false;
        }
    }
}
