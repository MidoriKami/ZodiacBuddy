using System;
using System.Numerics;

using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using ZodiacBuddy.Novus.Data;

namespace ZodiacBuddy.Novus;

/// <summary>
/// Novus information window.
/// </summary>
public class NovusWindow
{
    private readonly NovusConfiguration novusConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="NovusWindow"/> class.
    /// </summary>
    /// <param name="novusConfiguration">Configuration for Novus relic.</param>
    public NovusWindow(NovusConfiguration novusConfiguration)
    {
        this.novusConfiguration = novusConfiguration;
    }

    /// <summary>
    /// Draw information window.
    /// </summary>
    public void Draw()
    {
        if (!this.novusConfiguration.DisplayNovusInfo) return;

        InventoryItem[] slot =
        {
            NovusManager.GetEquippedItem(0),
            NovusManager.GetEquippedItem(1),
        };
        if (!NovusRelic.Novus.ContainsKey(slot[0].ItemID) &&
            !NovusRelic.Novus.ContainsKey(slot[1].ItemID)) return;

        if (ImGui.Begin(
            "Novus Information",
            ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar))
        {
            this.DisplayRelicInfo(slot[0]);
            this.DisplayRelicInfo(slot[1]);
            this.DisplayBonusLight();
            ImGui.SetWindowSize("Novus Information", Vector2.Zero);
        }

        ImGui.End();
    }

    private static string FixMissingPolice(string message)
    {
        return message
            .Replace("Œ", "Oe")
            .Replace("œ", "oe");
    }

    private void DisplayRelicInfo(InventoryItem item)
    {
        if (!NovusRelic.Novus.TryGetValue(item.ItemID, out var relicInfo)) return;
        var name = FixMissingPolice(relicInfo);
        ImGui.Text(name);
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, this.novusConfiguration.ProgressColor);
        var progressBarVector = this.novusConfiguration.DisplayBonusDuty
            ? new Vector2(ImGui.GetWindowContentRegionWidth(), 0f)
            : new Vector2(130, 0f);
        ImGui.ProgressBar(item.Spiritbond / 2000f, progressBarVector, $"{item.Spiritbond}/2000");

        ImGui.PopStyleColor();
    }

    private void DisplayBonusLight()
    {
        if (!this.novusConfiguration.DisplayBonusDuty) return;

        // Don't display bonus light detected 2h ago
        var dt = DateTime.UtcNow.Subtract(TimeSpan.FromHours(2));
        if (this.novusConfiguration.LightBonusDetection != null &&
            this.novusConfiguration.LightBonusTerritoryId != null &&
            this.novusConfiguration.LightBonusDetection.Value > dt)
        {
            var dutyName = NovusDuty.Dictionary[this.novusConfiguration.LightBonusTerritoryId.Value].DutyName;
            var detectionDate = this.novusConfiguration.LightBonusDetection.Value.ToLocalTime().ToString("t");
            ImGui.Text($"Light bonus detected at {detectionDate} on duty");
            ImGui.Text($"\"{dutyName}\"");
        }
        else
        {
            ImGui.Text("No duty with light bonus detected");
        }
    }
}