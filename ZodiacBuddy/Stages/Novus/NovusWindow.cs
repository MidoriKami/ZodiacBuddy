using System.Numerics;

using Dalamud.Interface;
using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using ZodiacBuddy.BonusLight;
using ZodiacBuddy.Stages.Brave;

namespace ZodiacBuddy.Stages.Novus;

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

    private static BonusLightConfiguration Configuration => Service.Configuration.BonusLight;

    /// <summary>
    /// Draw information window.
    /// </summary>
    public void Draw()
    {
        if (!this.ShowWindow)
            return;

        var name = "Novus Zodiac Information";
        var flags = ImGuiWindowFlags.NoResize
            | ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoScrollbar;
        if (ImGui.Begin(name, flags))
        {
            this.DisplayRelicInfo(this.MainhandItem);
            this.DisplayRelicInfo(this.OffhandItem);
            this.DisplayBonusLight();

            ImGui.SetWindowSize(name, Vector2.Zero);
        }

        ImGui.End();
    }

    private void DisplayRelicInfo(InventoryItem item)
    {
        if (!NovusRelic.Items.TryGetValue(item.ItemID, out var name))
            return;

        name = name
            .Replace("Œ", "Oe")
            .Replace("œ", "oe");
        ImGui.Text(name);

        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, Configuration.ProgressColor);
        var progressBarVector = Configuration.DisplayBonusDuty
            ? new Vector2(ImGui.GetWindowContentRegionWidth(), 0f)
            : new Vector2(130, 0f);

        var value = item.Spiritbond;
        var progress = value / 2000f;
        ImGui.ProgressBar(progress, progressBarVector, $"{value}/2000");

        ImGui.PopStyleColor();
    }

    private void DisplayBonusLight()
    {
        if (!Configuration.DisplayBonusDuty)
            return;

        if (Configuration.LightBonusDetection != null &&
            Configuration.LightBonusTerritoryId != null)
        {
            var dutyName = BonusLightDuty.GetValue(Configuration.LightBonusTerritoryId.Value).DutyName
                .Replace("Œ", "Oe")
                .Replace("œ", "oe");
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