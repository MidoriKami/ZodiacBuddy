using System;
using System.Numerics;

using Dalamud.Game.Text;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;

namespace ZodiacBuddy;

/// <summary>
/// Plugin configuration window.
/// </summary>
internal class ConfigWindow : Window {
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigWindow"/> class.
    /// </summary>
    public ConfigWindow() : base("Zodiac Buddy Setup")
    {
        this.RespectCloseHotkey = true;

        this.SizeCondition = ImGuiCond.FirstUseEver;
        this.Size = new Vector2(740, 490);
    }

    /// <inheritdoc/>
    public override void Draw() {
        if (ImGui.CollapsingHeader("General"))
            this.DrawGeneral();

        if (ImGui.CollapsingHeader("Interface"))
            this.DrawInterface();

        if (ImGui.CollapsingHeader("Bonus Light"))
            this.DrawBonusLight();

        if (ImGui.CollapsingHeader("Atma"))
            this.DrawAtma();

        if (ImGui.CollapsingHeader("Novus"))
            this.DrawNovus();

        if (ImGui.CollapsingHeader("Brave"))
            this.DrawBrave();
        
        if (Service.Interface.IsDevMenuOpen && ImGui.CollapsingHeader("Debug"))
            this.Debug();
    }

    private void DrawGeneral() {
        var names = Enum.GetNames<XivChatType>();
        var channels = Enum.GetValues<XivChatType>();
        var current = Array.IndexOf(channels, Service.Configuration.ChatType);
        if (current == -1) {
            current = Array.IndexOf(channels, Service.Configuration.ChatType = XivChatType.Echo);
            Service.Configuration.Save();
        }

        ImGui.SetNextItemWidth(200f);
        if (ImGui.Combo("Chat channel", ref current, names, names.Length)) {
            Service.Configuration.ChatType = channels[current];
            Service.Configuration.Save();
        }

        ImGui.Spacing();

        if (ImGui.Checkbox("Disable Teleport", ref Service.Configuration.DisableTeleport)) {
            Service.Configuration.Save();
        }
    }

    private void DrawInterface() {
        var manualSize = Service.Configuration.InformationWindow.ManualSize;
        if (ImGui.Checkbox("Manual size for relic information", ref manualSize)) {
            Service.Configuration.InformationWindow.ManualSize = manualSize;
            Service.Configuration.Save();
        }

        var clickThrough = Service.Configuration.InformationWindow.ClickThrough;
        if (ImGui.Checkbox("Click through relic information", ref clickThrough)) {
            Service.Configuration.InformationWindow.ClickThrough = clickThrough;
            Service.Configuration.Save();
        }

        ImGui.PushItemWidth(150f);
        var progressSize = Service.Configuration.InformationWindow.ProgressSize;
        if (ImGui.SliderInt("Light progress size ", ref progressSize, 80, 500)) {
            Service.Configuration.InformationWindow.ProgressSize = progressSize;
            Service.Configuration.Save();
        }

        ImGui.SameLine();
        var progressAutoSize = Service.Configuration.InformationWindow.ProgressAutoSize;
        if (ImGui.Checkbox("Automatic", ref progressAutoSize)) {
            Service.Configuration.InformationWindow.ProgressAutoSize = progressAutoSize;
            Service.Configuration.Save();
        }

        var progressColor = ImGui.ColorConvertU32ToFloat4(Service.Configuration.InformationWindow.ProgressColor);
        if (ImGui.ColorEdit4("Light progress color", ref progressColor, ImGuiColorEditFlags.DisplayHex | ImGuiColorEditFlags.PickerHueWheel)) {
            Service.Configuration.InformationWindow.ProgressColor = ImGui.ColorConvertFloat4ToU32(progressColor);
            Service.Configuration.Save();
        }

        ImGui.PopItemWidth();
        ImGui.SameLine();
        if (ImGui.Button("Reset")) {
            Service.Configuration.InformationWindow.ResetProgressColor();
            Service.Configuration.Save();
        }

        ImGui.Spacing();
    }

    private void DrawBonusLight() {
        string status;
        Vector4 statusColor;
        if (Service.BonusLightManager.LastRequestIsSuccess) {
            status = "OK";
            statusColor = ImGuiColors.HealerGreen;
        }
        else {
            status = "Error";
            statusColor = ImGuiColors.DalamudRed;
        }

        ImGui.Text("Info: The bonus light is crowdsourced.");
        ImGui.Text("Server status: ");
        ImGui.SameLine();
        ImGui.TextColored(statusColor, status);
        ImGui.Spacing();

        var displayBonusDuty = Service.Configuration.BonusLight.DisplayBonusDuty;
        if (ImGui.Checkbox("Display duty with the bonus of light in relic info windows", ref displayBonusDuty)) {
            Service.Configuration.BonusLight.DisplayBonusDuty = displayBonusDuty;
            Service.Configuration.Save();
        }

        ImGui.Separator();

        var notifyBonusDuty = Service.Configuration.BonusLight.NotifyLightBonusOnlyWhenEquipped;
        if (ImGui.Checkbox("Notify duty with bonus only when an applicable relic is equipped", ref notifyBonusDuty)) {
            Service.Configuration.BonusLight.NotifyLightBonusOnlyWhenEquipped = notifyBonusDuty;
            Service.Configuration.Save();
        }

        var playSound = Service.Configuration.BonusLight.PlaySoundOnLightBonusNotification;
        if (ImGui.Checkbox("Play sound when notifying about light bonus", ref playSound)) {
            Service.Configuration.BonusLight.PlaySoundOnLightBonusNotification = playSound;
            Service.Configuration.Save();
        }

        ImGui.SetNextItemWidth(150f);
        var soundId = Service.Configuration.BonusLight.LightBonusNotificationSound;
        if (ImGui.SliderInt("##LightBonusSound", ref soundId, 1, 16, "<se.%d>")) {
            Service.Configuration.BonusLight.LightBonusNotificationSound = soundId;
            Service.Configuration.Save();
        }

        ImGui.SameLine();
        if (ImGui.Button("Play sound##LightBonusSound"))
            UIModule.PlayChatSoundEffect((uint)soundId);

        ImGui.Spacing();
    }

    private void DrawAtma() {
        ImGui.Text("Pro tip: Use the Sonar plugin to track Fate uptime across your entire datacenter.\n");

        var braveEcho = Service.Configuration.BraveEchoTarget;
        if (ImGui.Checkbox("Display target selection in chat", ref braveEcho)) {
            Service.Configuration.BraveEchoTarget = braveEcho;
            Service.Configuration.Save();
        }

        ImGui.Spacing();
    }

    private void DrawNovus() {
        var showRelicWindow = Service.Configuration.Novus.DisplayRelicInfo;
        if (ImGui.Checkbox("Display Novus relic information when equipped", ref showRelicWindow)) {
            Service.Configuration.Novus.DisplayRelicInfo = showRelicWindow;
            Service.Configuration.Save();
        }

        var skipAnimation = Service.Configuration.Novus.DontPlayRelicGlassAnimation;
        if (ImGui.Checkbox("Skip text animation from the relic glass", ref skipAnimation)) {
            Service.Configuration.Novus.DontPlayRelicGlassAnimation = skipAnimation;
            Service.Configuration.Save();
        }

        var showNumbers = Service.Configuration.Novus.ShowNumbersInRelicGlass;
        if (ImGui.Checkbox("Show light numbers in the relic glass", ref showNumbers)) {
            Service.Configuration.Novus.ShowNumbersInRelicGlass = showNumbers;
            Service.Configuration.Save();
        }

        ImGui.Spacing();
    }

    private void DrawBrave() {
        var showRelicWindow = Service.Configuration.Brave.DisplayRelicInfo;
        if (ImGui.Checkbox("Display Zodiac Brave relic information when equipped", ref showRelicWindow)) {
            Service.Configuration.Brave.DisplayRelicInfo = showRelicWindow;
            Service.Configuration.Save();
        }

        var skipAnimation = Service.Configuration.Brave.DontPlayRelicMagiciteAnimation;
        if (ImGui.Checkbox("Skip text animation from the relic magicite", ref skipAnimation)) {
            Service.Configuration.Brave.DontPlayRelicMagiciteAnimation = skipAnimation;
            Service.Configuration.Save();
        }

        var showNumbers = Service.Configuration.Brave.ShowNumbersInRelicMagicite;
        if (ImGui.Checkbox("Show light numbers in the relic magicite", ref showNumbers)) {
            Service.Configuration.Brave.ShowNumbersInRelicMagicite = showNumbers;
            Service.Configuration.Save();
        }

        ImGui.Spacing();
    }
    
    private void Debug() {
        if (ImGui.Button("Check duties territory"))
            DebugTools.CheckBonusLightDutyTerritories();
    }
}