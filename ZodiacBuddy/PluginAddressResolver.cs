using System;

using Dalamud.Game;
using Dalamud.Logging;

namespace ZodiacBuddy
{
    /// <summary>
    /// Plugin address resolver.
    /// </summary>
    internal class PluginAddressResolver : BaseAddressResolver
    {
        private const string RelicNoteBookReceiveEventSignature = // Client::UI::AddonRelicNoteBook.ReceiveEvent
            "48 89 74 24 ?? 57 48 83 EC 20 8D 42 FD 49 8B F0 48 8B F9 83 F8 09 77 2D";

        private const string AgentContentsFinderOpenRegularDuty = // AgentContentsFinder.OpenRegularDuty
            "48 89 6C 24 ?? 48 89 74 24 ?? 57 48 81 EC ?? ?? ?? ?? 48 8B F9 41 0F B6 E8";

        /// <summary>
        /// Gets the address of the RelicNoteBook addon's ReceiveEvent method.
        /// </summary>
        public IntPtr AddonRelicNoteBookReceiveEventAddress { get; private set; }

        /// <summary>
        /// Gets the address of the AgentContentsFinder's OpenRegularDuty method.
        /// </summary>
        public IntPtr AgentContentsFinderOpenRegularDutyAddress { get; private set; }

        /// <inheritdoc/>
        protected override void Setup64Bit(SigScanner scanner)
        {
            this.AddonRelicNoteBookReceiveEventAddress = scanner.ScanText(RelicNoteBookReceiveEventSignature);
            this.AgentContentsFinderOpenRegularDutyAddress = scanner.ScanText(AgentContentsFinderOpenRegularDuty);

            PluginLog.Verbose("===== Z O D I A C B U D D Y =====");
            PluginLog.Verbose($"{nameof(this.AddonRelicNoteBookReceiveEventAddress)} {this.AddonRelicNoteBookReceiveEventAddress:X}");
            PluginLog.Verbose($"{nameof(this.AgentContentsFinderOpenRegularDutyAddress)} {this.AgentContentsFinderOpenRegularDutyAddress:X}");
        }
    }
}
