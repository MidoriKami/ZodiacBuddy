using System;
using System.Numerics;

using Dalamud.Interface;
using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using ZodiacBuddy.BonusLight;

namespace ZodiacBuddy.InformationWindow;

/// <summary>
/// Default information window.
/// </summary>
public abstract class InformationWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InformationWindow"/> class.
    /// </summary>
    /// <param name="name">Name of the window.</param>
    protected InformationWindow(string name)
    {
        this.Name = name;
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

    private static BonusLightConfiguration BonusConfiguration => Service.Configuration.BonusLight;

    private static InformationWindowConfiguration InfoWindowConfiguration => Service.Configuration.InformationWindow;

    /// <summary>
    /// Gets name of the information window.
    /// </summary>
    private string Name { get; }

    /// <summary>
    /// Draw information window.
    /// </summary>
    public void Draw()
    {
        if (!this.ShowWindow)
            return;

        var flags = ImGuiWindowFlags.NoFocusOnAppearing |
                    ImGuiWindowFlags.NoTitleBar |
                    ImGuiWindowFlags.NoScrollbar;

        flags |= InfoWindowConfiguration.ClickThrough
            ? ImGuiWindowFlags.NoInputs
            : ImGuiWindowFlags.None;

        if (ImGui.Begin(this.Name, flags))
        {
            this.DisplayRelicInfo(this.MainhandItem);
            this.DisplayRelicInfo(this.OffhandItem);
            this.DisplayBonusLight();

            if (!InfoWindowConfiguration.ManualSize)
            {
                ImGui.SetWindowSize(this.Name, Vector2.Zero);
            }
        }

        ImGui.End();
    }

    /// <summary>
    /// Display information about the specified relic.
    /// </summary>
    /// <param name="item">Relic to display.</param>
    protected abstract void DisplayRelicInfo(InventoryItem item);

    /// <summary>
    /// Determine the size of the progress bar.
    /// </summary>
    /// <param name="relicName">Name of the relic.</param>
    /// <returns>Vector2 of the determined size.</returns>
    protected Vector2 DetermineProgressSize(string relicName)
    {
        if (!InfoWindowConfiguration.ProgressAutoSize)
            return Vector2.Zero with { X = InfoWindowConfiguration.ProgressSize };

        if (InfoWindowConfiguration.ManualSize ||
            (BonusConfiguration.DisplayBonusDuty &&
             BonusConfiguration.ActiveBonus.Count > 0))
            return Vector2.Zero with { X = ImGui.GetContentRegionAvail().X - 1 };

        var vector = ImGui.CalcTextSize(relicName) with { Y = 0f };

        return vector.X < 80 ?
            new Vector2(80, 0) :
            vector;
    }

    private void DisplayBonusLight()
    {
        if (!BonusConfiguration.DisplayBonusDuty)
            return;

        if (BonusConfiguration.ActiveBonus.Count > 0)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.TextColored(ImGuiColors.DalamudYellow, FontAwesomeIcon.Lightbulb.ToIconString());
            ImGui.PopFont();

            var timeOfDay = DateTime.Now.TimeOfDay;
            var startEvenHour = timeOfDay.Hours % 2 == 0
                ? TimeSpan.FromHours(timeOfDay.Hours)
                : TimeSpan.FromHours(timeOfDay.Hours - 1);
            var startWindowDate = startEvenHour.ToString(@"hh\:mm");
            var endWindowDate = startEvenHour.Add(TimeSpan.FromHours(2)).ToString(@"hh\:mm");
            ImGui.SameLine();
            ImGui.Text($"{startWindowDate} - {endWindowDate}");

            foreach (var territoryId in BonusConfiguration.ActiveBonus)
            {
                var dutyName = BonusLightDuty.GetValue(territoryId).DutyName
                    .Replace("Œ", "Oe")
                    .Replace("œ", "oe");
                ImGui.Text($"\"{dutyName}\"");
            }
        }
    }
}