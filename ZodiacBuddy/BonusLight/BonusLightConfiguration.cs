using System;

using Newtonsoft.Json;

namespace ZodiacBuddy.BonusLight;

/// <summary>
/// Configuration class for Nexus relic.
/// </summary>
public class BonusLightConfiguration
{
    /// <summary>
    /// Gets or sets the Territory Id of the current duty with bonus of light.
    /// </summary>
    [JsonIgnore]
    public uint? LightBonusTerritoryId { get; set; }

    /// <summary>
    /// Gets or sets UTC time of detection of the current duty with bonus of light.
    /// </summary>
    [JsonIgnore]
    public DateTime? LightBonusDetection { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to display the current duty with light bonus on the Novus information window.
    /// </summary>
    public bool DisplayBonusDuty { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to notify the user of new duty with bonus light when the relic is not equipped.
    /// </summary>
    public bool NotifyLightBonusOnlyWhenEquipped { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether play sound when notifying about the bonus of light.
    /// </summary>
    public bool PlaySoundOnLightBonusNotification { get; set; } = true;

    /// <summary>
    /// Gets or sets the sound to play when notifying about the bonus of light.
    /// </summary>
    public int LightBonusNotificationSound { get; set; } = 9;
}