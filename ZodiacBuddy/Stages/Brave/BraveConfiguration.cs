namespace ZodiacBuddy.Stages.Brave;

/// <summary>
/// Configuration class for Zodiac Brave relic.
/// </summary>
public class BraveConfiguration {
    /// <summary>
    /// Gets or sets a value indicating whether to display the information about the equipped relic.
    /// </summary>
    public bool DisplayRelicInfo { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show the actual numbers in the RelicMagicite addon.
    /// </summary>
    public bool ShowNumbersInRelicMagicite { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to not display the first message on the RelicMagicite addon.
    /// </summary>
    public bool DontPlayRelicMagiciteAnimation { get; set; } = true;
}