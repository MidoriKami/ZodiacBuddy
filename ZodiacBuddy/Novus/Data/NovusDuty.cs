using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;

namespace ZodiacBuddy.Novus.Data;

/// <summary>
/// Define a duty susceptible to have the light bonus.
/// </summary>
[SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1025:Code should not contain multiple whitespace in a row", Justification = "I like this better")]
[SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1008:Opening parenthesis should be spaced correctly", Justification = "I like this better")]
public class NovusDuty
{
    /// <summary>
    /// Get the list of duty susceptible to have the light bonus.
    /// </summary>
    public static readonly Dictionary<uint, NovusDuty> Dictionary = new()
    {
        // Hard Trials (& Urth's Fount)
        {  292, new NovusDuty( 292, 16) }, // the Bowl of Embers (Hard)
        {  294, new NovusDuty( 294, 16) }, // the Howling Eye (Hard)
        {  293, new NovusDuty( 293, 16) }, // the Navel (Hard)
        {  207, new NovusDuty( 207, 16) }, // Thornmarch (Hard)
        {  281, new NovusDuty( 281, 16) }, // the Whorleater (Hard)
        {  374, new NovusDuty( 374, 16) }, // the Striking Tree (Hard)
        {  377, new NovusDuty( 377, 16) }, // the Akh Afah Amphitheatre (Hard)
        {  394, new NovusDuty( 394, 16) }, // Urth's Fount
        { 1048, new NovusDuty(1048, 16) }, // The Porta Decumana

        // Raids
        { 241, new NovusDuty(241, 32) },   // the Binding Coil of Bahamut - Turn 1
        { 242, new NovusDuty(242, 32) },   // the Binding Coil of Bahamut - Turn 2
        { 244, new NovusDuty(244, 16) },   // the Binding Coil of Bahamut - Turn 4
        { 245, new NovusDuty(245, 32) },   // the Binding Coil of Bahamut - Turn 5
        { 355, new NovusDuty(355, 32) },   // the Second Coil of Bahamut - Turn 1
        { 356, new NovusDuty(356, 32) },   // the Second Coil of Bahamut - Turn 2
        { 357, new NovusDuty(357, 32) },   // the Second Coil of Bahamut - Turn 3
        { 358, new NovusDuty(358, 32) },   // the Second Coil of Bahamut - Turn 4
        { 380, new NovusDuty(380, 32) },   // the Second Coil of Bahamut (Savage) - Turn 1
        { 381, new NovusDuty(381, 32) },   // the Second Coil of Bahamut (Savage) - Turn 2
        { 382, new NovusDuty(382, 32) },   // the Second Coil of Bahamut (Savage) - Turn 3
        { 383, new NovusDuty(383, 32) },   // the Second Coil of Bahamut (Savage) - Turn 4
        { 193, new NovusDuty(193, 32) },   // the Final Coil of Bahamut - Turn 1
        { 194, new NovusDuty(194, 32) },   // the Final Coil of Bahamut - Turn 2
        { 195, new NovusDuty(195, 32) },   // the Final Coil of Bahamut - Turn 3
        { 196, new NovusDuty(196, 32) },   // the Final Coil of Bahamut - Turn 4

        // Extreme Trials
        { 348, new NovusDuty(348, 16) },   // the Minstrel's Ballad: Ultima's B
        { 295, new NovusDuty(295, 32) },   // the Bowl of Embers (Extreme)
        { 297, new NovusDuty(297, 16) },   // the Howling Eye (Extreme)
        { 296, new NovusDuty(296, 32) },   // the Navel (Extreme)
        { 364, new NovusDuty(364, 32) },   // Thornmarch (Extreme)
        { 359, new NovusDuty(359, 32) },   // the Whorleater (Extreme)
        { 375, new NovusDuty(375, 32) },   // the Striking Tree (Extreme)
        { 378, new NovusDuty(378, 32) },   // the Akh Afah Amphitheatre (Extreme)

        // Dungeons
        { 1036, new NovusDuty(1036, 48) }, // Sastasha
        { 1037, new NovusDuty(1037, 48) }, // The Tam-Tara Deepcroft
        { 1038, new NovusDuty(1038, 48) }, // Copperbell Mines
        {  162, new NovusDuty( 162, 48) }, // Halatali
        { 1039, new NovusDuty(1039, 48) }, // The Thousand Maws of Toto-Rak
        { 1040, new NovusDuty(1040, 48) }, // Haukke Manor
        { 1041, new NovusDuty(1041, 48) }, // Brayflox's Longstop
        {  163, new NovusDuty( 163, 48) }, // The Sunken Temple of Qarn
        {  170, new NovusDuty( 170, 48) }, // Cutter's Cry
        { 1042, new NovusDuty(1042, 48) }, // Stone Vigil
        {  171, new NovusDuty( 171, 48) }, // Dzemael Darkhold
        {  172, new NovusDuty( 172, 48) }, // Aurum Vale
        {  159, new NovusDuty( 159, 48) }, // the Wanderer's Palace
        { 1043, new NovusDuty(1043, 48) }, // Castrum Meridianum
        { 1044, new NovusDuty(1044, 48) }, // the Praetorium
        {  167, new NovusDuty( 167, 48) }, // Amdapor Keep
        {  160, new NovusDuty( 160, 48) }, // Pharos Sirius
        {  349, new NovusDuty( 349, 48) }, // Copperbell Mines (Hard)
        {  350, new NovusDuty( 350, 48) }, // Haukke Manor (Hard)
        {  363, new NovusDuty( 363, 48) }, // the Lost City of Amdapor
        {  360, new NovusDuty( 360, 48) }, // Halatali (Hard)
        {  362, new NovusDuty( 362, 48) }, // Brayflox's Longstop (Hard)
        {  361, new NovusDuty( 361, 48) }, // Hullbreaker Isle
        {  373, new NovusDuty( 373, 48) }, // the Tam–Tara Deepcroft (Hard)
        {  365, new NovusDuty( 365, 48) }, // the Stone Vigil (Hard)
        {  371, new NovusDuty( 371, 48) }, // Snowcloak
        {  387, new NovusDuty( 387, 48) }, // Sastasha (Hard)
        {  367, new NovusDuty( 367, 48) }, // the Sunken Temple of Qarn (Hard)
        {  150, new NovusDuty( 150, 48) }, // the Keeper of the Lake
        {  188, new NovusDuty( 188, 48) }, // the Wanderer's Palace (Hard)
        {  189, new NovusDuty( 189, 48) }, // Amdapor Keep (Hard)

        // PVP
        { 376, new NovusDuty(376, 48) }, // The Borderland Ruins (Secure)

        // Alliance Raid
        { 151, new NovusDuty(151, 48) }, // The World of Darkness
        { 174, new NovusDuty(174, 96) }, // Labyrinth of the Ancients
        { 372, new NovusDuty(372, 96) }, // Syrcus Tower
    };

    private NovusDuty(uint territoryId, uint defaultLightIntensity)
    {
        this.DefaultLightIntensity = defaultLightIntensity;
        var territory = Service.DataManager.Excel.GetSheet<TerritoryType>()!.GetRow(territoryId)!;
        this.DutyName = territory.ContentFinderCondition.Value!.Name.ToDalamudString().ToString()!;
    }

    /// <summary>
    /// Gets the name of the duty.
    /// </summary>
    public string DutyName { get; }

    /// <summary>
    /// Gets the default light intensity of the duty.
    /// </summary>
    public uint DefaultLightIntensity { get; }
}