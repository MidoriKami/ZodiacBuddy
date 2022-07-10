using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace ZodiacBuddy.Stages.Atma.Data;

/// <summary>
/// A single target for a Trial of the Braves book.
/// </summary>
internal struct BraveTarget
{
    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the issuer name for leves.
    /// </summary>
    public string Issuer { get; init; }

    /// <summary>
    /// Gets the zone name.
    /// </summary>
    public string ZoneName { get; init; }

    /// <summary>
    /// Gets the zone ID.
    /// </summary>
    public uint ZoneID { get; init; }

    /// <summary>
    /// Gets the contents finder condition ID.
    /// </summary>
    public uint ContentsFinderConditionID { get; init; }

    /// <summary>
    /// Gets the location name.
    /// </summary>
    public string LocationName { get; init; }

    /// <summary>
    /// Gets the position that this target is roughly at.
    /// </summary>
    public MapLinkPayload Position { get; init; }
}
