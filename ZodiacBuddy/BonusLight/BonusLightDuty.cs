using System.Collections.Generic;

using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;

namespace ZodiacBuddy.BonusLight;

/// <summary>
/// Define a duty susceptible to have the light bonus.
/// </summary>
public class BonusLightDuty
{
    // key: Territory ID
    private static readonly Dictionary<uint, BonusLightDuty> Dataset = new()
    {
        #pragma warning disable format,SA1008,SA1025
        // Hard Trials and Urth's Fount
        {  292, new( 292, 16) }, // the Bowl of Embers (Hard)
        {  294, new( 294, 16) }, // the Howling Eye (Hard)
        {  293, new( 293, 16) }, // the Navel (Hard)
        {  207, new( 207, 16) }, // Thornmarch (Hard)
        {  281, new( 281, 16) }, // the Whorleater (Hard)
        {  374, new( 374, 16) }, // the Striking Tree (Hard)
        {  377, new( 377, 16) }, // the Akh Afah Amphitheatre (Hard)
        {  394, new( 394, 16) }, // Urth's Fount
        { 1048, new(1048, 16) }, // The Porta Decumana

        // Raids
        { 241, new(241, 32) }, // the Binding Coil of Bahamut - Turn 1
        { 242, new(242, 32) }, // the Binding Coil of Bahamut - Turn 2
        { 244, new(244, 16) }, // the Binding Coil of Bahamut - Turn 4
        { 245, new(245, 32) }, // the Binding Coil of Bahamut - Turn 5
        { 355, new(355, 32) }, // the Second Coil of Bahamut - Turn 1
        { 356, new(356, 32) }, // the Second Coil of Bahamut - Turn 2
        { 357, new(357, 32) }, // the Second Coil of Bahamut - Turn 3
        { 358, new(358, 32) }, // the Second Coil of Bahamut - Turn 4
        { 380, new(380, 32) }, // the Second Coil of Bahamut (Savage) - Turn 1
        { 381, new(381, 32) }, // the Second Coil of Bahamut (Savage) - Turn 2
        { 382, new(382, 32) }, // the Second Coil of Bahamut (Savage) - Turn 3
        { 383, new(383, 32) }, // the Second Coil of Bahamut (Savage) - Turn 4
        { 193, new(193, 32) }, // the Final Coil of Bahamut - Turn 1
        { 194, new(194, 32) }, // the Final Coil of Bahamut - Turn 2
        { 195, new(195, 32) }, // the Final Coil of Bahamut - Turn 3
        { 196, new(196, 32) }, // the Final Coil of Bahamut - Turn 4

        // Extreme Trials
        { 348, new(348, 16) }, // the Minstrel's Ballad: Ultima's Bane
        { 295, new(295, 32) }, // the Bowl of Embers (Extreme)
        { 297, new(297, 16) }, // the Howling Eye (Extreme)
        { 296, new(296, 32) }, // the Navel (Extreme)
        { 364, new(364, 32) }, // Thornmarch (Extreme)
        { 359, new(359, 32) }, // the Whorleater (Extreme)
        { 375, new(375, 32) }, // the Striking Tree (Extreme)
        { 378, new(378, 32) }, // the Akh Afah Amphitheatre (Extreme)

        // Dungeons
        { 1036, new(1036, 48) }, // Sastasha
        { 1037, new(1037, 48) }, // The Tam-Tara Deepcroft
        { 1038, new(1038, 48) }, // Copperbell Mines
        {  162, new( 162, 48) }, // Halatali
        { 1039, new(1039, 48) }, // The Thousand Maws of Toto-Rak
        { 1040, new(1040, 48) }, // Haukke Manor
        { 1041, new(1041, 48) }, // Brayflox's Longstop
        {  163, new( 163, 48) }, // The Sunken Temple of Qarn
        {  170, new( 170, 48) }, // Cutter's Cry
        { 1042, new(1042, 48) }, // Stone Vigil
        {  171, new( 171, 48) }, // Dzemael Darkhold
        {  172, new( 172, 48) }, // Aurum Vale
        {  159, new( 159, 48) }, // the Wanderer's Palace
        { 1043, new(1043, 48) }, // Castrum Meridianum
        { 1044, new(1044, 48) }, // the Praetorium
        {  167, new( 167, 48) }, // Amdapor Keep
        {  160, new( 160, 48) }, // Pharos Sirius
        {  349, new( 349, 48) }, // Copperbell Mines (Hard)
        {  350, new( 350, 48) }, // Haukke Manor (Hard)
        {  363, new( 363, 48) }, // the Lost City of Amdapor
        {  360, new( 360, 48) }, // Halatali (Hard)
        {  362, new( 362, 48) }, // Brayflox's Longstop (Hard)
        {  361, new( 361, 48) }, // Hullbreaker Isle
        {  373, new( 373, 48) }, // the Tam–Tara Deepcroft (Hard)
        {  365, new( 365, 48) }, // the Stone Vigil (Hard)
        {  371, new( 371, 48) }, // Snowcloak
        {  387, new( 387, 48) }, // Sastasha (Hard)
        {  367, new( 367, 48) }, // the Sunken Temple of Qarn (Hard)
        {  150, new( 150, 48) }, // the Keeper of the Lake
        {  188, new( 188, 48) }, // the Wanderer's Palace (Hard)
        {  189, new( 189, 48) }, // Amdapor Keep (Hard)

        // PVP
        {  376, new(376, 48) }, // The Borderland Ruins (Secure)

        // Alliance Raids
        {  151, new(151, 96) }, // The World of Darkness
        {  174, new(174, 48) }, // Labyrinth of the Ancients
        {  372, new(372, 96) }, // Syrcus Tower
        #pragma warning restore format,SA1008,SA1025
    };

    private BonusLightDuty(uint territoryId, uint defaultLightIntensity)
    {
        this.DefaultLightIntensity = defaultLightIntensity;

        this.DutyName = Service.DataManager.Excel.GetSheet<TerritoryType>()!
            .GetRow(territoryId)!
            .ContentFinderCondition.Value!.Name
            .ToDalamudString()
            .ToString()!;
    }

    /// <summary>
    /// Gets the name of the duty.
    /// </summary>
    public string DutyName { get; }

    /// <summary>
    /// Gets the default light intensity of the duty.
    /// </summary>
    public uint DefaultLightIntensity { get; }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="territoryID">Territory ID.</param>
    /// <returns>Novus duty data.</returns>
    public static BonusLightDuty GetValue(uint territoryID)
        => Dataset[territoryID];

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="territoryID">Territory ID.</param>
    /// <param name="duty">Novus duty data.</param>
    /// <returns>True if the duty was found, otherwise false.</returns>
    public static bool TryGetValue(uint territoryID, out BonusLightDuty? duty)
        => Dataset.TryGetValue(territoryID, out duty);
}
