using System;

namespace ZodiacBuddy.Novus;

/// <summary>
/// Configuration class for Nexus relic.
/// </summary>
public class NovusConfiguration
{
    /// <summary>
    /// Default color for the Novus weapons information progress bar.
    /// </summary>
    [NonSerialized]
    private static readonly uint DefaultProgressColor = 0xFF943463;

    /// <summary>
    /// Gets or sets a value indicating whether to display the information about the Novus weapons.
    /// </summary>
    public bool DisplayNovusInfo { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to display the current duty with light bonus on the Novus information window.
    /// </summary>
    public bool DisplayBonusDuty { get; set; } = true;

    /// <summary>
    /// Gets or sets the color of the progress bar on Novus weapons information.
    /// </summary>
    public uint ProgressColor { get; set; } = DefaultProgressColor;

    /// <summary>
    /// Gets or sets the Territory Id of the current duty with bonus of light.
    /// </summary>
    public uint? LightBonusTerritoryId { get; set; }

    /// <summary>
    /// Gets or sets UTC time of detection of the current duty with bonus of light.
    /// </summary>
    public DateTime? LightBonusDetection { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show the actual numbers in the RelicGlass addon.
    /// </summary>
    public bool ShowNumbersInRelicGlass { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to notify the user of new duty with bonus light when the relic is not equipped.
    /// </summary>
    public bool NotifyLightBonusOnlyWhenEquipped { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to not display the first message on the RelicGlass addon.
    /// </summary>
    public bool DontPlayRelicGlassAnimation { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether play sound when notifying about the bonus of light.
    /// </summary>
    public bool PlaySoundOnLightBonusNotification { get; set; } = true;

    /// <summary>
    /// Reset ProgressColor to it's default value.
    /// </summary>
    public void ResetProgressColor()
    {
        this.ProgressColor = DefaultProgressColor;
    }
}