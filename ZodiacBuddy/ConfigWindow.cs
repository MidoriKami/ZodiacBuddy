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
            if (ImGui.CollapsingHeader("General"))
                this.DrawGeneral();

            if (ImGui.CollapsingHeader("Atma"))
                this.DrawAtma();

            if (ImGui.CollapsingHeader("Novus"))
                this.DrawNovus();
        }

        private void DrawGeneral()
        {
            var names = Enum.GetNames<XivChatType>();
            var channels = Enum.GetValues<XivChatType>();
            var current = Array.IndexOf(channels, Service.Configuration.ChatType);
            if (current == -1)
            {
                current = Array.IndexOf(channels, Service.Configuration.ChatType = XivChatType.Echo);
                Service.Configuration.Save();
            }

            ImGui.SetNextItemWidth(200f);
            if (ImGui.Combo("Chat channel", ref current, names, names.Length))
            {
                Service.Configuration.ChatType = channels[current];
                Service.Configuration.Save();
            }

            ImGui.NewLine();
        }

        private void DrawAtma()
        {
            ImGui.Text("Pro tip: Use the Sonar plugin to track Fate uptime across your entire datacenter.\n");

            var braveEcho = Service.Configuration.BraveEchoTarget;
            if (ImGui.Checkbox("Display target selection in chat", ref braveEcho))
            {
                Service.Configuration.BraveEchoTarget = braveEcho;
                Service.Configuration.Save();
            }

            ImGui.NewLine();
        }

        private void DrawNovus()
        {
            var display = Service.Configuration.NovusConfiguration.DisplayNovusInfo;
            if (ImGui.Checkbox("Display Novus weapon information when equipped", ref display))
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

            ImGui.Separator();

            var notifyBonusDuty = Service.Configuration.NovusConfiguration.NotifyLightBonusOnlyWhenEquipped;
            if (ImGui.Checkbox("Notify duty with bonus only when Novus relic is equipped", ref notifyBonusDuty))
            {
                Service.Configuration.NovusConfiguration.NotifyLightBonusOnlyWhenEquipped = notifyBonusDuty;
                Service.Configuration.Save();
            }

            var playSound = Service.Configuration.NovusConfiguration.PlaySoundOnLightBonusNotification;
            if (ImGui.Checkbox("Play sound when notifying about light bonus", ref playSound))
            {
                Service.Configuration.NovusConfiguration.PlaySoundOnLightBonusNotification = playSound;
                Service.Configuration.Save();
            }

            ImGui.Separator();

            var dontPlayRelicGlassAnimation = Service.Configuration.NovusConfiguration.DontPlayRelicGlassAnimation;
            if (ImGui.Checkbox("Skip text animation from the relic glass", ref dontPlayRelicGlassAnimation))
            {
                Service.Configuration.NovusConfiguration.DontPlayRelicGlassAnimation = dontPlayRelicGlassAnimation;
                Service.Configuration.Save();
            }

            var showNumbersInRelicGlass = Service.Configuration.NovusConfiguration.ShowNumbersInRelicGlass;
            if (ImGui.Checkbox("Show light numbers in the relic glass", ref showNumbersInRelicGlass))
            {
                Service.Configuration.NovusConfiguration.ShowNumbersInRelicGlass = showNumbersInRelicGlass;
                Service.Configuration.Save();
            }

            ImGui.PushItemWidth(150f);
            var progressColor = ImGui.ColorConvertU32ToFloat4(Service.Configuration.NovusConfiguration.ProgressColor);
            if (ImGui.ColorEdit4("Light progress color", ref progressColor, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.PickerHueWheel))
            {
                Service.Configuration.NovusConfiguration.ProgressColor = ImGui.ColorConvertFloat4ToU32(progressColor);
                Service.Configuration.Save();
            }

            ImGui.SameLine();
            if (ImGui.Button("Reset"))
            {
                Service.Configuration.NovusConfiguration.ResetProgressColor();
                Service.Configuration.Save();
            }

            ImGui.NewLine();
        }
    }
}
