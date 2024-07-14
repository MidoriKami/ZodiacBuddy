using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;

namespace ZodiacBuddy.BonusLight;

/// <summary>
/// Define the quantity of light gain and it's log message.
/// </summary>
public class BonusLightLevel {
    /// <summary>
    /// Initializes a new instance of the <see cref="BonusLightLevel"/> class.
    /// </summary>
    /// <param name="intensity">Light intensity.</param>
    /// <param name="rowId">Log messageID.</param>
    public BonusLightLevel(uint intensity, uint rowId) {
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
