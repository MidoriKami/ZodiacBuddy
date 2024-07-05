using System.Collections.Generic;

using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;

namespace ZodiacBuddy.Stages.Brave;

/// <summary>
/// Define the relic item Id and their names.
/// </summary>
public static class BraveRelic
{
    /// <summary>
    /// List of Zodiac Brave weapons.
    /// </summary>
    public static readonly Dictionary<uint, string> Items = new()
    {
        { 9491, GetItemName(9491) }, // Excalibur
        { 9492, GetItemName(9492) }, // Kaiser Knuckles
        { 9493, GetItemName(9493) }, // Ragnarok
        { 9494, GetItemName(9494) }, // Longinus
        { 9495, GetItemName(9495) }, // Yoichi Bow
        { 9496, GetItemName(9496) }, // Nirvana
        { 9497, GetItemName(9497) }, // Lilith Rod
        { 9498, GetItemName(9498) }, // Apocalypse
        { 9499, GetItemName(9499) }, // Last Resort
        { 9500, GetItemName(9500) }, // Aegis Shield
        { 9501, GetItemName(9501) }, // Sasuke's Blades
    };

    private static string GetItemName(uint ItemId)
    {
        return Service.DataManager.Excel.GetSheet<Item>()!
            .GetRow(ItemId)!.Name
            .ToDalamudString()
            .ToString();
    }
}