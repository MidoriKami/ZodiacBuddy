using System;
using System.Numerics;

using Dalamud.Game.Text;
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

            var names = Enum.GetNames<XivChatType>();
            var channels = Enum.GetValues<XivChatType>();
            var current = Array.IndexOf(channels, Service.Configuration.BraveEchoChannel);
            if (current == -1)
            {
                current = Array.IndexOf(channels, Service.Configuration.BraveEchoChannel = XivChatType.Echo);
                Service.Configuration.Save();
            }

            if (ImGui.Combo("Channel", ref current, names, names.Length))
            {
                Service.Configuration.BraveEchoChannel = channels[current];
                Service.Configuration.Save();
            }

            DrawNovus();
        }

        private static void DrawNovus()
        {
            if (!ImGui.CollapsingHeader("Novus relic")) return;

            var display = Service.Configuration.NovusConfiguration.DisplayNovusInfo;
            if (ImGui.Checkbox("Display Novus weapon information", ref display))
            {
                Service.Configuration.NovusConfiguration.DisplayNovusInfo = display;
                Service.Configuration.Save();
            }

            var displayBonusDuty = Service.Configuration.NovusConfiguration.DisplayBonusDuty;
            if (ImGui.Checkbox("Display duty with the bonus of light", ref displayBonusDuty))
            {
                Service.Configuration.NovusConfiguration.DisplayBonusDuty = displayBonusDuty;
                Service.Configuration.Save();
            }

            var playSound = Service.Configuration.NovusConfiguration.PlaySoundOnLightBonusNotification;
            if (ImGui.Checkbox("Play sound when notifying about light bonus", ref playSound))
            {
                Service.Configuration.NovusConfiguration.PlaySoundOnLightBonusNotification = playSound;
                Service.Configuration.Save();
            }

            var dontPlayRelicGlassAnimation = Service.Configuration.NovusConfiguration.DontPlayRelicGlassAnimation;
            if (ImGui.Checkbox(
                    "Skip text animation from the relic glass (Kind of)",
                    ref dontPlayRelicGlassAnimation))
            {
                Service.Configuration.NovusConfiguration.DontPlayRelicGlassAnimation = dontPlayRelicGlassAnimation;
                Service.Configuration.Save();
            }

            if (ImGui.CollapsingHeader("Light progress color"))
            {
                var vector = ImGui.ColorConvertU32ToFloat4(Service.Configuration.NovusConfiguration.ProgressColor);
                if (ImGui.ColorPicker4("##LightProgressColor", ref vector, ImGuiColorEditFlags.None))
                {
                    Service.Configuration.NovusConfiguration.ProgressColor = ImGui.ColorConvertFloat4ToU32(vector);
                    Service.Configuration.Save();
                }

                if (ImGui.Button("Reset color"))
                {
                    Service.Configuration.NovusConfiguration.ResetProgressColor();
                    Service.Configuration.Save();
                }
            }
        }
    }
}
