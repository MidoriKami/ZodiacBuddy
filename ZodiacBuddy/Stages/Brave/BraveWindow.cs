using System;
using System.Numerics;

using Dalamud.Interface;
using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using ZodiacBuddy.BonusLight;

namespace ZodiacBuddy.Stages.Brave;

/// <summary>
/// Novus information window.
/// </summary>
public class BraveWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BraveWindow"/> class.
    /// </summary>
    public BraveWindow()
    {
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show this window.
    /// </summary>
    public bool ShowWindow { get; set; } = false;

    /// <summary>
    /// Gets or sets the mainhand item.
    /// </summary>
    public InventoryItem MainhandItem { get; set; } = default;

    /// <summary>
    /// Gets or sets the offhand item.
    /// </summary>
    public InventoryItem OffhandItem { get; set; } = default;

    private static BonusLightConfiguration Configuration => Service.Configuration.BonusLight;

    /// <summary>
    /// Draw information window.
    /// </summary>
    public void Draw()
    {
        if (!this.ShowWindow)
            return;

        var name = "Zodiac Brave Information";
        var flags = ImGuiWindowFlags.NoResize
            | ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoScrollbar;
        if (ImGui.Begin(name, flags))
        {
            this.DisplayRelicInfo(this.MainhandItem);
            this.DisplayRelicInfo(this.OffhandItem);
            this.DisplayBonusLight();

            // Shrink
            ImGui.SetWindowSize(name, Vector2.Zero);

            // Set minimum size.
            var sz = ImGui.GetWindowSize();
            ImGui.SetWindowSize(name, new Vector2(Math.Max(sz.X, 100f), sz.Y));
        }

        ImGui.End();
    }

    private void DisplayRelicInfo(InventoryItem item)
    {
        if (!BraveRelic.Items.TryGetValue(item.ItemID, out var name))
            return;

        name = name
            .Replace("Œ", "Oe")
            .Replace("œ", "oe");
        ImGui.Text(name);

        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, Configuration.ProgressColor);
        var progressBarVector = Configuration.DisplayBonusDuty
            ? new Vector2(ImGui.GetWindowContentRegionWidth(), 0f)
            : new Vector2(130, 0f);

        var mahatmaValue = (item.Spiritbond / 500) + 1;
        if (item.Spiritbond == 0)
            mahatmaValue = 0;

        var mahatmaProgress = mahatmaValue / 12f;
        ImGui.ProgressBar(mahatmaProgress, progressBarVector, $"{mahatmaValue}/12");

        var value = item.Spiritbond % 500;
        if (value == 1)
            value -= 1;

        var progress = value / 80f;
        ImGui.ProgressBar(progress, progressBarVector, $"{value / 2}/40");

        ImGui.PopStyleColor();
    }

    private void DisplayBonusLight()
    {
        if (!Configuration.DisplayBonusDuty)
            return;

        if (Configuration.LightBonusDetection != null &&
            Configuration.LightBonusTerritoryId != null)
        {
            var dutyName = BonusLightDuty.GetValue(Configuration.LightBonusTerritoryId.Value).DutyName;
            var detectionDate = Configuration.LightBonusDetection.Value.ToLocalTime().ToString("t");

            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
            ImGui.Text(FontAwesomeIcon.Lightbulb.ToIconString());
            ImGui.PopStyleColor();
            ImGui.PopFont();

            ImGui.SameLine();
            ImGui.Text($"{detectionDate} \"{dutyName}\"");
        }
    }
}