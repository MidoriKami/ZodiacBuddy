using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;

namespace ZodiacBuddy.Novus.Data;

/// <summary>
/// Define the quantity of light gain and it's toast message.
/// </summary>
public class LightLevel
{
    /// <summary>
    /// List of light level and the displayed toast.
    /// </summary>
    public static readonly LightLevel[] Values =
    {
        #pragma warning disable format,SA1008,SA1025
        new(  8, 4649), // Feeble
        new( 16, 4650), // Gentle
        new( 32, 4651), // Bright
        new( 48, 4652), // Brilliant
        new( 96, 4653), // Blinding
        new(128, 4654), // Newborn Star
        #pragma warning restore format,SA1008,SA1025
    };

    private LightLevel(uint intensity, uint rowId)
    {
        this.Intensity = intensity;
        this.Message = Service.DataManager.Excel.GetSheet<LogMessage>()!
            .GetRow(rowId)!
            .Text.ToDalamudString()
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