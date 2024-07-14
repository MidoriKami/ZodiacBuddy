using System;

using Newtonsoft.Json;

namespace ZodiacBuddy.BonusLight;

/// <summary>
/// Report struct to communicate with the server.
/// </summary>
public struct Report {
    /// <summary>
    /// Initializes a new instance of the <see cref="Report"/> struct.
    /// </summary>
    /// <param name="datacenterId">Datacenter id.</param>
    /// <param name="worldId">World id.</param>
    /// <param name="territoryId">Territory id.</param>
    /// <param name="date">Date of the report.</param>
    public Report(uint datacenterId, uint worldId, uint territoryId, DateTime date) {
        this.DatacenterId = datacenterId;
        this.WorldId = worldId;
        this.TerritoryId = territoryId;
        this.Date = date;
    }

    /// <summary>
    /// Gets or sets datacenter id.
    /// </summary>
    [JsonProperty("datacenter_id")] public uint DatacenterId { get; set; }

    /// <summary>
    /// Gets or sets world id.
    /// </summary>
    [JsonProperty("world_id")] public uint WorldId { get; set; }

    /// <summary>
    /// Gets or sets territory id.
    /// </summary>
    [JsonProperty("territory_id")] public uint TerritoryId { get; set; }

    /// <summary>
    /// Gets or sets report date.
    /// </summary>
    [JsonProperty("date")] public DateTime Date { get; set; }
}