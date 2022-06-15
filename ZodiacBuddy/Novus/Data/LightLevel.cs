using System.Diagnostics.CodeAnalysis;

using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;

namespace ZodiacBuddy.Novus.Data;

/// <summary>
/// Define the quantity of light gain and it's toast message.
/// </summary>
[SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1025:Code should not contain multiple whitespace in a row", Justification = "I like this better")]
[SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1008:Opening parenthesis should be spaced correctly", Justification = "I like this better")]
public class LightLevel
{
    // LightReward
    // Feeble = 8
    // Gentle = 16
    // Bright = 32
    // Brilliant = 48
    // Blinding = 96
    // NewbornStar = 128

    /// <summary>
    /// List of light level and the displayed toast.
    /// </summary>
    public static readonly LightLevel[] Values =
    {
        new(  8, 4649),
        new( 16, 4650),
        new( 32, 4651),
        new( 48, 4652),
        new( 96, 4653),
        new(128, 4654),
    };

    private LightLevel(uint intensity, uint rowId)
    {
        this.Intensity = intensity;
        this.Message = Service.DataManager.Excel.GetSheet<LogMessage>()!.GetRow(rowId)!.Text.ToDalamudString()
            .ToString().Trim();
    }

    /// <summary>
    /// Gets the intensity of light.
    /// </summary>
    public uint Intensity { get; }

    /// <summary>
    /// Gets the toast message.
    /// </summary>
    public string Message { get; }
}