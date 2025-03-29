using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using ZodiacBuddy.InformationWindow;

namespace ZodiacBuddy.Stages.Novus;

/// <summary>
/// Novus information window.
/// </summary>
public class NovusWindow() : InformationWindow.InformationWindow("Novus Zodiac Information") {
    private static InformationWindowConfiguration InfoWindowConfiguration => Service.Configuration.InformationWindow;

    /// <inheritdoc/>
    protected override void DisplayRelicInfo(InventoryItem item) {
        if (!NovusRelic.Items.TryGetValue(item.ItemId, out var name))
            return;

        name = name
            .Replace("Œ", "Oe")
            .Replace("œ", "oe");
        ImGui.Text(name);

        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, InfoWindowConfiguration.ProgressColor);

        var value = item.SpiritbondOrCollectability;
        var progress = value / 2000f;
        ImGui.ProgressBar(progress, DetermineProgressSize(name), $"{value}/2000");

        ImGui.PopStyleColor();
    }
}