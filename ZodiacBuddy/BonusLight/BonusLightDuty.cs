using System.Collections.Generic;
using Lumina.Excel.Sheets;

namespace ZodiacBuddy.BonusLight;

/// <summary>
/// Define a duty susceptible to have the light bonus.
/// </summary>
public class BonusLightDuty {
    // key: Territory ID
    private static readonly Dictionary<uint, BonusLightDuty> Dataset = new() {
        #pragma warning disable format,SA1008,SA1025
        // Hard Trials and Urth's Fount
        {  292, new BonusLightDuty( 292, 16) }, // the Bowl of Embers (Hard)
        {  294, new BonusLightDuty( 294, 16) }, // the Howling Eye (Hard)
        {  293, new BonusLightDuty( 293, 16) }, // the Navel (Hard)
        { 1067, new BonusLightDuty(1067, 16) }, // Thornmarch (Hard)
        {  281, new BonusLightDuty( 281, 16) }, // the Whorleater (Hard)
        {  374, new BonusLightDuty( 374, 16) }, // the Striking Tree (Hard)
        {  377, new BonusLightDuty( 377, 16) }, // the Akh Afah Amphitheatre (Hard)
        {  394, new BonusLightDuty( 394, 16) }, // Urth's Fount
        { 1048, new BonusLightDuty(1048, 16) }, // The Porta Decumana

        // Raids
        { 241, new BonusLightDuty(241, 32) }, // the Binding Coil of Bahamut - Turn 1
        { 242, new BonusLightDuty(242, 32) }, // the Binding Coil of Bahamut - Turn 2
        { 244, new BonusLightDuty(244, 16) }, // the Binding Coil of Bahamut - Turn 4
        { 245, new BonusLightDuty(245, 32) }, // the Binding Coil of Bahamut - Turn 5
        { 355, new BonusLightDuty(355, 32) }, // the Second Coil of Bahamut - Turn 1
        { 356, new BonusLightDuty(356, 32) }, // the Second Coil of Bahamut - Turn 2
        { 357, new BonusLightDuty(357, 32) }, // the Second Coil of Bahamut - Turn 3
        { 358, new BonusLightDuty(358, 32) }, // the Second Coil of Bahamut - Turn 4
        { 380, new BonusLightDuty(380, 32) }, // the Second Coil of Bahamut (Savage) - Turn 1
        { 381, new BonusLightDuty(381, 32) }, // the Second Coil of Bahamut (Savage) - Turn 2
        { 382, new BonusLightDuty(382, 32) }, // the Second Coil of Bahamut (Savage) - Turn 3
        { 383, new BonusLightDuty(383, 32) }, // the Second Coil of Bahamut (Savage) - Turn 4
        { 193, new BonusLightDuty(193, 32) }, // the Final Coil of Bahamut - Turn 1
        { 194, new BonusLightDuty(194, 32) }, // the Final Coil of Bahamut - Turn 2
        { 195, new BonusLightDuty(195, 32) }, // the Final Coil of Bahamut - Turn 3
        { 196, new BonusLightDuty(196, 32) }, // the Final Coil of Bahamut - Turn 4

        // Extreme Trials
        { 348, new BonusLightDuty(348, 16) }, // the Minstrel's Ballad: Ultima's Bane
        { 295, new BonusLightDuty(295, 32) }, // the Bowl of Embers (Extreme)
        { 297, new BonusLightDuty(297, 16) }, // the Howling Eye (Extreme)
        { 296, new BonusLightDuty(296, 32) }, // the Navel (Extreme)
        { 364, new BonusLightDuty(364, 32) }, // Thornmarch (Extreme)
        { 359, new BonusLightDuty(359, 32) }, // the Whorleater (Extreme)
        { 375, new BonusLightDuty(375, 32) }, // the Striking Tree (Extreme)
        { 378, new BonusLightDuty(378, 32) }, // the Akh Afah Amphitheatre (Extreme)

        // Dungeons
        { 1036, new BonusLightDuty(1036, 48) }, // Sastasha
        { 1037, new BonusLightDuty(1037, 48) }, // The Tam-Tara Deepcroft
        { 1038, new BonusLightDuty(1038, 48) }, // Copperbell Mines
        {  162, new BonusLightDuty( 162, 48) }, // Halatali
        { 1039, new BonusLightDuty(1039, 48) }, // The Thousand Maws of Toto-Rak
        { 1040, new BonusLightDuty(1040, 48) }, // Haukke Manor
        { 1041, new BonusLightDuty(1041, 48) }, // Brayflox's Longstop
        {  163, new BonusLightDuty( 163, 48) }, // The Sunken Temple of Qarn
        {  170, new BonusLightDuty( 170, 48) }, // Cutter's Cry
        { 1042, new BonusLightDuty(1042, 48) }, // Stone Vigil
        {  171, new BonusLightDuty( 171, 48) }, // Dzemael Darkhold
        {  172, new BonusLightDuty( 172, 48) }, // Aurum Vale
        {  159, new BonusLightDuty( 159, 48) }, // the Wanderer's Palace
        { 1043, new BonusLightDuty(1043, 48) }, // Castrum Meridianum
        { 1044, new BonusLightDuty(1044, 48) }, // the Praetorium
        {  167, new BonusLightDuty( 167, 48) }, // Amdapor Keep
        {  160, new BonusLightDuty( 160, 48) }, // Pharos Sirius
        {  349, new BonusLightDuty( 349, 48) }, // Copperbell Mines (Hard)
        {  350, new BonusLightDuty( 350, 48) }, // Haukke Manor (Hard)
        {  363, new BonusLightDuty( 363, 48) }, // the Lost City of Amdapor
        {  360, new BonusLightDuty( 360, 48) }, // Halatali (Hard)
        {  362, new BonusLightDuty( 362, 32) }, // Brayflox's Longstop (Hard)
        {  361, new BonusLightDuty( 361, 48) }, // Hullbreaker Isle
        {  373, new BonusLightDuty( 373, 48) }, // the Tam–Tara Deepcroft (Hard)
        {  365, new BonusLightDuty( 365, 48) }, // the Stone Vigil (Hard)
        { 1062, new BonusLightDuty(1062, 48) }, // Snowcloak
        {  387, new BonusLightDuty( 387, 48) }, // Sastasha (Hard)
        {  367, new BonusLightDuty( 367, 48) }, // the Sunken Temple of Qarn (Hard)
        { 1063, new BonusLightDuty(1063, 48) }, // the Keeper of the Lake
        {  188, new BonusLightDuty( 188, 48) }, // the Wanderer's Palace (Hard)
        {  189, new BonusLightDuty( 189, 48) }, // Amdapor Keep (Hard)

        // PVP
        {  376, new BonusLightDuty(376, 48) }, // The Borderland Ruins (Secure)

        // Alliance Raids
        {  151, new BonusLightDuty(151, 96) }, // The World of Darkness
        {  174, new BonusLightDuty(174, 48) }, // Labyrinth of the Ancients
        {  372, new BonusLightDuty(372, 96) }, // Syrcus Tower
        #pragma warning restore format,SA1008,SA1025
    };

    private BonusLightDuty(uint territoryId, uint defaultLightIntensity) {
        this.DefaultLightIntensity = defaultLightIntensity;

        this.DutyName = Service.DataManager.Excel.GetSheet<TerritoryType>()
	        .GetRow(territoryId)
	        .ContentFinderCondition.Value.Name.ExtractText();
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
    /// <param name="territoryId">Territory ID.</param>
    /// <returns>Novus duty data.</returns>
    public static BonusLightDuty GetValue(uint territoryId)
        => Dataset[territoryId];

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="territoryId">Territory ID.</param>
    /// <param name="duty">Novus duty data.</param>
    /// <returns>True if the duty was found, otherwise false.</returns>
    public static bool TryGetValue(uint territoryId, out BonusLightDuty? duty)
        => Dataset.TryGetValue(territoryId, out duty);

    /// <summary>
    /// Gets BonusLightDuty list.
    /// </summary>
    /// <returns>Dataset of the BonusLightDuty.</returns>
    public static Dictionary<uint, BonusLightDuty> GetDataset() => Dataset;
}
