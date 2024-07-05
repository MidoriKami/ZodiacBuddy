using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using ZodiacBuddy.InformationWindow;

namespace ZodiacBuddy.Stages.Brave;

/// <summary>
/// Brave information window.
/// </summary>
public class BraveWindow : InformationWindow.InformationWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BraveWindow"/> class.
    /// </summary>
    public BraveWindow()
        : base("Zodiac Brave Information")
    {
    }

    private static InformationWindowConfiguration InfoWindowConfiguration => Service.Configuration.InformationWindow;

    /// <inheritdoc/>
    protected override void DisplayRelicInfo(InventoryItem item)
    {
        if (!BraveRelic.Items.TryGetValue(item.ItemId, out var name))
            return;

        name = name
            .Replace("Œ", "Oe")
            .Replace("œ", "oe");
        ImGui.Text(name);

        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, InfoWindowConfiguration.ProgressColor);

        var mahatmaValue = (item.Spiritbond / 500) + 1;
        if (item.Spiritbond == 0)
            mahatmaValue = 0;

        var mahatmaProgress = mahatmaValue / 12f;
        ImGui.ProgressBar(mahatmaProgress, this.DetermineProgressSize(name), $"{mahatmaValue}/12");

        var value = item.Spiritbond % 500;
        if (value == 1)
            value -= 1;

        var progress = value / 80f;
        ImGui.ProgressBar(progress, this.DetermineProgressSize(name), $"{value / 2}/40");

        ImGui.PopStyleColor();
    }
}