using System.Numerics;

using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace ZodiacBuddy
{
    /// <summary>
    /// Plugin configuration window.
    /// </summary>
    internal class ConfigWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigWindow"/> class.
        /// </summary>
        public ConfigWindow()
            : base("Zodiac Buddy Setup")
        {
            this.RespectCloseHotkey = true;

            this.SizeCondition = ImGuiCond.FirstUseEver;
            this.Size = new Vector2(740, 490);
        }

        /// <inheritdoc/>
        public override void Draw()
        {
            ImGui.TextWrapped("Currently the only implemented feature is teleporting to your target when clicking on pictures in the Trial of the Braves books.\n\nEnjoy!\n");

            var braveEcho = Service.Configuration.BraveEchoTarget;
            if (ImGui.Checkbox("Echo Brave target selection to chat", ref braveEcho))
            {
                Service.Configuration.BraveEchoTarget = braveEcho;
                Service.Configuration.Save();
            }
        }
    }
}
