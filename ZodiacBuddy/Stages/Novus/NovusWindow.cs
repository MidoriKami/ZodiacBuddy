using System;
using System.Numerics;

using Dalamud.Interface;
using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using ZodiacBuddy.Novus.Data;

namespace ZodiacBuddy.Novus;

/// <summary>
/// Novus information window.
/// </summary>
public class NovusWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NovusWindow"/> class.
    /// </summary>
    public NovusWindow()
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

    private static NovusConfiguration Configuration => Service.Configuration.NovusConfiguration;

    /// <summary>
    /// Draw information window.
    /// </summary>
    public void Draw()
    {
        if (!this.ShowWindow)
            return;

        if (ImGui.Begin(
            "Novus Information",
            ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar))
        {
            this.DisplayRelicInfo(this.MainhandItem);
            this.DisplayRelicInfo(this.OffhandItem);
            this.DisplayBonusLight();

            ImGui.SetWindowSize("Novus Information", Vector2.Zero);
        }

        ImGui.End();
    }

    private void DisplayRelicInfo(InventoryItem item)
    {
        if (!NovusRelic.Novus.TryGetValue(item.ItemID, out var name))
            return;

        name = name
            .Replace("Œ", "Oe")
            .Replace("œ", "oe");
        ImGui.Text(name);

        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, Configuration.ProgressColor);
        var progressBarVector = Configuration.DisplayBonusDuty
            ? new Vector2(ImGui.GetWindowContentRegionWidth(), 0f)
            : new Vector2(130, 0f);
        ImGui.ProgressBar(item.Spiritbond / 2000f, progressBarVector, $"{item.Spiritbond}/2000");

        ImGui.PopStyleColor();
    }

    private void DisplayBonusLight()
    {
        if (!Configuration.DisplayBonusDuty)
            return;

        // Don't display bonus light detected 2h ago
        var dt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(2));
        if (Configuration.LightBonusDetection != null &&
            Configuration.LightBonusTerritoryId != null &&
            Configuration.LightBonusDetection.Value > dt)
        {
            var dutyName = NovusDuty.GetValue(Configuration.LightBonusTerritoryId.Value).DutyName;
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